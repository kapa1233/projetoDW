using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebServicos.Data;
using WebServicos.Models;

namespace WebServicos.API.Controllers
{
    /// <summary>
    /// API REST para gestão de pedidos da plataforma WebServicos.
    /// Disponibiliza endpoints para consulta e atualização de pedidos.
    /// Endpoint base: /api/pedidos
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class PedidosController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<PedidosController> _logger;

        /// <summary>
        /// Construtor com injeção de dependências (contexto da BD e logger).
        /// </summary>
        public PedidosController(ApplicationDbContext db, ILogger<PedidosController> logger)
        {
            _db = db;
            _logger = logger;
        }

        /// <summary>
        /// Obtém a lista de todos os pedidos, com filtros opcionais por cliente e/ou estado.
        /// Inclui informação do cliente e dos serviços associados.
        /// GET /api/pedidos?clienteId=xxx&amp;estado=1
        /// </summary>
        /// <param name="clienteId">ID do cliente para filtrar (opcional).</param>
        /// <param name="estado">Estado do pedido para filtrar (opcional).</param>
        [HttpGet]
        public async Task<ActionResult> GetPedidos([FromQuery] string? clienteId, [FromQuery] EstadoPedido? estado)
        {
            var query = _db.Pedidos
                .Include(p => p.Cliente)
                .Include(p => p.PedidoServicos)
                    .ThenInclude(ps => ps.Servico)
                .AsQueryable();

            // Aplicar filtros opcionais via query string
            if (!string.IsNullOrEmpty(clienteId))
                query = query.Where(p => p.ClienteId == clienteId);

            if (estado.HasValue)
                query = query.Where(p => p.Estado == estado.Value);

            // Projeção para DTO anónimo (evita serialização circular)
            var pedidos = await query
                .OrderByDescending(p => p.DataPedido)
                .Select(p => new
                {
                    p.Id,
                    p.TituloProjeto,
                    p.Estado,
                    EstadoNome = p.Estado.ToString(),
                    p.DataPedido,
                    p.OrcamentoTotal,
                    Cliente = p.Cliente != null ? new { p.Cliente.NomeCompleto, p.Cliente.Email } : null,
                    Servicos = p.PedidoServicos.Select(ps => ps.Servico != null ? ps.Servico.Nome : "").ToList()
                })
                .ToListAsync();

            return Ok(pedidos);
        }

        /// <summary>
        /// Obtém os detalhes completos de um pedido específico pelo seu ID.
        /// Inclui informação do cliente e de todos os serviços associados.
        /// GET /api/pedidos/{id}
        /// </summary>
        /// <param name="id">ID do pedido a consultar.</param>
        [HttpGet("{id:int}")]
        public async Task<ActionResult> GetPedido(int id)
        {
            // Eager loading das relações necessárias
            var pedido = await _db.Pedidos
                .Include(p => p.Cliente)
                .Include(p => p.PedidoServicos)
                    .ThenInclude(ps => ps.Servico)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (pedido == null)
                return NotFound(new { Mensagem = $"Pedido #{id} não encontrado." });

            return Ok(new
            {
                pedido.Id,
                pedido.TituloProjeto,
                pedido.Descricao,
                pedido.Estado,
                EstadoNome = pedido.Estado.ToString(),
                pedido.DataPedido,
                pedido.PrazoEstimado,
                pedido.OrcamentoTotal,
                pedido.Observacoes,
                Cliente = pedido.Cliente != null
                    ? new { pedido.Cliente.Id, pedido.Cliente.NomeCompleto, pedido.Cliente.Email }
                    : null,
                Servicos = pedido.PedidoServicos.Select(ps => new
                {
                    ps.ServicoId,
                    Nome = ps.Servico?.Nome,
                    ps.PrecoAcordado,
                    ps.Notas
                })
            });
        }

        /// <summary>
        /// Atualiza o estado de um pedido existente.
        /// PATCH /api/pedidos/{id}/estado
        /// </summary>
        /// <param name="id">ID do pedido a atualizar.</param>
        /// <param name="dto">Objeto com o novo estado a aplicar.</param>
        [HttpPatch("{id:int}/estado")]
        public async Task<IActionResult> AlterarEstado(int id, [FromBody] AlterarEstadoDto dto)
        {
            var pedido = await _db.Pedidos.FindAsync(id);
            if (pedido == null)
                return NotFound(new { Mensagem = $"Pedido #{id} não encontrado." });

            pedido.Estado = dto.NovoEstado;
            await _db.SaveChangesAsync();

            _logger.LogInformation("API: Estado do pedido #{Id} alterado para {Estado}.", id, dto.NovoEstado);
            return Ok(new { Mensagem = "Estado atualizado com sucesso.", NovoEstado = dto.NovoEstado.ToString() });
        }
    }

    /// <summary>
    /// DTO (Data Transfer Object) para a operação de alteração de estado de um pedido.
    /// </summary>
    public class AlterarEstadoDto
    {
        /// <summary>Novo estado a aplicar ao pedido.</summary>
        public EstadoPedido NovoEstado { get; set; }
    }
}
