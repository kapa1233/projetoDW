using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebServicos.Data;
using WebServicos.Models;

namespace WebServicos.Controllers
{
    /// <summary>
    /// API REST para gestão de pedidos.
    /// Administradores acedem a todos os pedidos; clientes apenas aos seus.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PedidosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public PedidosController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET /api/pedidos
        /// <summary>Lista todos os pedidos. Admin vê todos; cliente vê apenas os seus.</summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PedidoResumoDto>>> Listar()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Administrador");

            var query = _context.Pedidos
                .Include(p => p.Cliente)
                .Include(p => p.PedidoServicos)
                .AsQueryable();

            if (!isAdmin)
                query = query.Where(p => p.ClienteId == userId);

            // Materializa primeiro para evitar tradução de métodos custom para SQL
            var raw = await query.OrderByDescending(p => p.DataPedido).ToListAsync();

            var pedidos = raw.Select(p => new PedidoResumoDto
            {
                Id = p.Id,
                TituloProjeto = p.TituloProjeto,
                Estado = p.Estado.ToString(),
                EstadoNome = EstadoNome(p.Estado),
                DataPedido = p.DataPedido,
                PrazoEstimado = p.PrazoEstimado,
                OrcamentoTotal = p.OrcamentoTotal,
                ClienteNome = p.Cliente?.NomeCompleto,
                ClienteEmail = p.Cliente?.Email,
                NumServicos = p.PedidoServicos.Count
            }).ToList();

            return Ok(pedidos);
        }

        // GET /api/pedidos/{id}
        /// <summary>Detalhe de um pedido, incluindo serviços e mensagens.</summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<PedidoDetalheDto>> Detalhe(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Administrador");

            var pedido = await _context.Pedidos
                .Include(p => p.Cliente)
                .Include(p => p.PedidoServicos).ThenInclude(ps => ps.Servico)
                .Include(p => p.Mensagens).ThenInclude(m => m.Remetente)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (pedido == null) return NotFound();
            if (!isAdmin && pedido.ClienteId != userId) return Forbid();

            // Mapeamento em memória (após materialização) para suportar métodos C# custom
            return Ok(new PedidoDetalheDto
            {
                Id = pedido.Id,
                TituloProjeto = pedido.TituloProjeto,
                Descricao = pedido.Descricao,
                Observacoes = pedido.Observacoes,
                Estado = pedido.Estado.ToString(),
                EstadoNome = EstadoNome(pedido.Estado),
                DataPedido = pedido.DataPedido,
                PrazoEstimado = pedido.PrazoEstimado,
                OrcamentoTotal = pedido.OrcamentoTotal,
                EnderecoHttp = pedido.EnderecoHttp,
                ClienteId = pedido.ClienteId,
                ClienteNome = pedido.Cliente?.NomeCompleto,
                ClienteEmail = pedido.Cliente?.Email,
                Servicos = pedido.PedidoServicos.Select(ps => new PedidoServicoDto
                {
                    ServicoId = ps.ServicoId,
                    ServicoNome = ps.Servico?.Nome ?? string.Empty,
                    Quantidade = ps.Quantidade,
                    PrecoAcordado = ps.PrecoAcordado,
                    Notas = ps.Notas
                }).ToList(),
                Mensagens = pedido.Mensagens.OrderBy(m => m.DataHora).Select(m => new MensagemDto
                {
                    Id = m.Id,
                    Conteudo = m.Conteudo,
                    DataHora = m.DataHora,
                    Lida = m.Lida,
                    RemetenteId = m.RemetenteId,
                    RemetenteNome = m.Remetente?.NomeCompleto
                }).ToList()
            });
        }

        // POST /api/pedidos
        /// <summary>Cria um novo pedido para o utilizador autenticado.</summary>
        [HttpPost]
        public async Task<ActionResult<PedidoResumoDto>> Criar([FromBody] CriarPedidoDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            if (dto.ServicoIds == null || !dto.ServicoIds.Any())
                return BadRequest(new { erro = "Deve selecionar pelo menos um serviço." });

            var servicos = await _context.Servicos
                .Where(s => dto.ServicoIds.Contains(s.Id) && s.Ativo)
                .ToListAsync();

            if (servicos.Count != dto.ServicoIds.Distinct().Count())
                return BadRequest(new { erro = "Um ou mais serviços não existem ou não estão ativos." });

            var pedido = new Pedido
            {
                TituloProjeto = dto.TituloProjeto,
                Descricao = dto.Descricao,
                Observacoes = dto.Observacoes,
                PrazoEstimado = dto.PrazoEstimado,
                ClienteId = userId,
                DataPedido = DateTime.UtcNow,
                Estado = EstadoPedido.Pendente,
                OrcamentoTotal = servicos.Sum(s => s.PrecoBase)
            };

            _context.Pedidos.Add(pedido);
            await _context.SaveChangesAsync();

            foreach (var servico in servicos)
            {
                _context.PedidoServicos.Add(new PedidoServico
                {
                    PedidoId = pedido.Id,
                    ServicoId = servico.Id,
                    Quantidade = 1,
                    PrecoAcordado = servico.PrecoBase
                });
            }
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(Detalhe), new { id = pedido.Id }, new PedidoResumoDto
            {
                Id = pedido.Id,
                TituloProjeto = pedido.TituloProjeto,
                Estado = pedido.Estado.ToString(),
                EstadoNome = EstadoNome(pedido.Estado),
                DataPedido = pedido.DataPedido,
                OrcamentoTotal = pedido.OrcamentoTotal,
                NumServicos = servicos.Count
            });
        }

        // PUT /api/pedidos/{id}
        /// <summary>
        /// Atualiza um pedido.
        /// Admin pode alterar estado e prazo; cliente pode alterar título, descrição e observações.
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Atualizar(int id, [FromBody] AtualizarPedidoDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Administrador");

            var pedido = await _context.Pedidos.FindAsync(id);
            if (pedido == null) return NotFound();
            if (!isAdmin && pedido.ClienteId != userId) return Forbid();

            if (isAdmin)
            {
                if (dto.Estado.HasValue) pedido.Estado = dto.Estado.Value;
                if (dto.PrazoEstimado.HasValue) pedido.PrazoEstimado = dto.PrazoEstimado;
                if (!string.IsNullOrWhiteSpace(dto.EnderecoHttp)) pedido.EnderecoHttp = dto.EnderecoHttp;
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(dto.TituloProjeto)) pedido.TituloProjeto = dto.TituloProjeto;
                if (!string.IsNullOrWhiteSpace(dto.Descricao)) pedido.Descricao = dto.Descricao;
                pedido.Observacoes = dto.Observacoes;
                if (dto.PrazoEstimado.HasValue) pedido.PrazoEstimado = dto.PrazoEstimado;
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE /api/pedidos/{id}
        /// <summary>Elimina um pedido. Apenas administradores.</summary>
        [HttpDelete("{id}")]
        [Authorize(Policy = "Administrador")]
        public async Task<IActionResult> Eliminar(int id)
        {
            var pedido = await _context.Pedidos
                .Include(p => p.PedidoServicos)
                .Include(p => p.Mensagens)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (pedido == null) return NotFound();

            _context.PedidoServicos.RemoveRange(pedido.PedidoServicos);
            _context.Mensagens.RemoveRange(pedido.Mensagens);
            _context.Pedidos.Remove(pedido);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET /api/pedidos/{id}/mensagens
        /// <summary>Lista as mensagens de chat de um pedido.</summary>
        [HttpGet("{id}/mensagens")]
        public async Task<ActionResult<IEnumerable<MensagemDto>>> ListarMensagens(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Administrador");

            var pedido = await _context.Pedidos.FindAsync(id);
            if (pedido == null) return NotFound();
            if (!isAdmin && pedido.ClienteId != userId) return Forbid();

            var mensagens = await _context.Mensagens
                .Where(m => m.PedidoId == id)
                .Include(m => m.Remetente)
                .OrderBy(m => m.DataHora)
                .Select(m => new MensagemDto
                {
                    Id = m.Id,
                    Conteudo = m.Conteudo,
                    DataHora = m.DataHora,
                    Lida = m.Lida,
                    RemetenteId = m.RemetenteId,
                    RemetenteNome = m.Remetente != null ? m.Remetente.NomeCompleto : null
                })
                .ToListAsync();

            return Ok(mensagens);
        }

        // POST /api/pedidos/{id}/mensagens
        /// <summary>Envia uma mensagem de chat num pedido.</summary>
        [HttpPost("{id}/mensagens")]
        public async Task<ActionResult<MensagemDto>> EnviarMensagem(int id, [FromBody] EnviarMensagemDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Administrador");

            if (userId == null) return Unauthorized();

            var pedido = await _context.Pedidos.FindAsync(id);
            if (pedido == null) return NotFound();
            if (!isAdmin && pedido.ClienteId != userId) return Forbid();

            if (string.IsNullOrWhiteSpace(dto.Conteudo))
                return BadRequest(new { erro = "A mensagem não pode estar vazia." });

            var mensagem = new Mensagem
            {
                PedidoId = id,
                RemetenteId = userId,
                Conteudo = dto.Conteudo.Trim(),
                DataHora = DateTime.UtcNow,
                Lida = false
            };

            _context.Mensagens.Add(mensagem);
            await _context.SaveChangesAsync();

            var remetente = await _userManager.FindByIdAsync(userId);

            return CreatedAtAction(nameof(ListarMensagens), new { id }, new MensagemDto
            {
                Id = mensagem.Id,
                Conteudo = mensagem.Conteudo,
                DataHora = mensagem.DataHora,
                Lida = mensagem.Lida,
                RemetenteId = mensagem.RemetenteId,
                RemetenteNome = remetente?.NomeCompleto
            });
        }

        private static string EstadoNome(EstadoPedido estado) => estado switch
        {
            EstadoPedido.Pendente => "Pendente",
            EstadoPedido.EmAnalise => "Em Análise",
            EstadoPedido.Aprovado => "Aprovado",
            EstadoPedido.EmDesenvolvimento => "Em Desenvolvimento",
            EstadoPedido.EmRevisao => "Em Revisão",
            EstadoPedido.Concluido => "Concluído",
            EstadoPedido.Cancelado => "Cancelado",
            _ => estado.ToString()
        };
    }

    // ── DTOs ──

    public class PedidoResumoDto
    {
        public int Id { get; set; }
        public string TituloProjeto { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public string EstadoNome { get; set; } = string.Empty;
        public DateTime DataPedido { get; set; }
        public DateTime? PrazoEstimado { get; set; }
        public decimal? OrcamentoTotal { get; set; }
        public string? ClienteNome { get; set; }
        public string? ClienteEmail { get; set; }
        public int NumServicos { get; set; }
    }

    public class PedidoDetalheDto : PedidoResumoDto
    {
        public string Descricao { get; set; } = string.Empty;
        public string? Observacoes { get; set; }
        public string? EnderecoHttp { get; set; }
        public string ClienteId { get; set; } = string.Empty;
        public List<PedidoServicoDto> Servicos { get; set; } = new();
        public List<MensagemDto> Mensagens { get; set; } = new();
    }

    public class PedidoServicoDto
    {
        public int ServicoId { get; set; }
        public string ServicoNome { get; set; } = string.Empty;
        public int Quantidade { get; set; }
        public decimal? PrecoAcordado { get; set; }
        public string? Notas { get; set; }
    }

    public class MensagemDto
    {
        public int Id { get; set; }
        public string Conteudo { get; set; } = string.Empty;
        public DateTime DataHora { get; set; }
        public bool Lida { get; set; }
        public string RemetenteId { get; set; } = string.Empty;
        public string? RemetenteNome { get; set; }
    }

    public class CriarPedidoDto
    {
        public string TituloProjeto { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public string? Observacoes { get; set; }
        public DateTime? PrazoEstimado { get; set; }
        public List<int> ServicoIds { get; set; } = new();
    }

    public class AtualizarPedidoDto
    {
        public string? TituloProjeto { get; set; }
        public string? Descricao { get; set; }
        public string? Observacoes { get; set; }
        public DateTime? PrazoEstimado { get; set; }
        public EstadoPedido? Estado { get; set; }
        public string? EnderecoHttp { get; set; }
    }

    public class EnviarMensagemDto
    {
        public string Conteudo { get; set; } = string.Empty;
    }
}
