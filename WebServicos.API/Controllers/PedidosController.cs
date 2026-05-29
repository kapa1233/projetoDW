using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebServicos.Data;
using WebServicos.Models;

namespace WebServicos.API.Controllers
{
    /// <summary>
    /// API REST para gestão de pedidos.
    /// Endpoint base: /api/pedidos
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class PedidosController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<PedidosController> _logger;

        public PedidosController(ApplicationDbContext db, ILogger<PedidosController> logger)
        {
            _db = db;
            _logger = logger;
        }

        // GET /api/pedidos
        [HttpGet]
        public async Task<ActionResult> GetPedidos([FromQuery] string? clienteId, [FromQuery] EstadoPedido? estado)
        {
            var query = _db.Pedidos
                .Include(p => p.Cliente)
                .Include(p => p.PedidoServicos)
                    .ThenInclude(ps => ps.Servico)
                .AsQueryable();

            if (!string.IsNullOrEmpty(clienteId))
                query = query.Where(p => p.ClienteId == clienteId);

            if (estado.HasValue)
                query = query.Where(p => p.Estado == estado.Value);

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

        // GET /api/pedidos/{id}
        [HttpGet("{id:int}")]
        public async Task<ActionResult> GetPedido(int id)
        {
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

        // PATCH /api/pedidos/{id}/estado
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

    public class AlterarEstadoDto
    {
        public EstadoPedido NovoEstado { get; set; }
    }
}
