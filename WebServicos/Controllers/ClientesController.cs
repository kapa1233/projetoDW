using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebServicos.Data;
using WebServicos.Models;

namespace WebServicos.Controllers
{
    /// <summary>
    /// API REST para gestão de clientes. Acesso exclusivo a administradores.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "Administrador")]
    public class ClientesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ClientesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET /api/clientes
        /// <summary>Lista todos os utilizadores com role Cliente.</summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ClienteDto>>> Listar()
        {
            var clientes = await _userManager.GetUsersInRoleAsync(DbSeeder.RoleCliente);

            var clienteIds = clientes.Select(c => c.Id).ToList();
            var pedidosPorCliente = await _context.Pedidos
                .Where(p => clienteIds.Contains(p.ClienteId))
                .GroupBy(p => p.ClienteId)
                .Select(g => new { ClienteId = g.Key, Total = g.Count() })
                .ToDictionaryAsync(x => x.ClienteId, x => x.Total);

            var resultado = clientes.Select(c => new ClienteDto
            {
                Id = c.Id,
                NomeCompleto = c.NomeCompleto,
                Email = c.Email ?? string.Empty,
                DataRegisto = c.DataRegisto,
                TotalPedidos = pedidosPorCliente.TryGetValue(c.Id, out var total) ? total : 0
            }).OrderBy(c => c.NomeCompleto).ToList();

            return Ok(resultado);
        }

        // GET /api/clientes/{id}
        /// <summary>Detalhe de um cliente, incluindo os seus pedidos.</summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ClienteDetalheDto>> Detalhe(string id)
        {
            var cliente = await _userManager.FindByIdAsync(id);
            if (cliente == null) return NotFound();

            var isCliente = await _userManager.IsInRoleAsync(cliente, DbSeeder.RoleCliente);
            if (!isCliente)
                return NotFound(new { erro = "Utilizador não é um cliente." });

            // Materializa primeiro, depois mapeia em memória (EstadoNome não é traduzível para SQL)
            var pedidosRaw = await _context.Pedidos
                .Where(p => p.ClienteId == id)
                .Include(p => p.PedidoServicos)
                .OrderByDescending(p => p.DataPedido)
                .ToListAsync();

            var pedidos = pedidosRaw.Select(p => new PedidoResumoClienteDto
            {
                Id = p.Id,
                TituloProjeto = p.TituloProjeto,
                Estado = p.Estado.ToString(),
                EstadoNome = EstadoPedidoHelper.EstadoNome(p.Estado),
                DataPedido = p.DataPedido,
                OrcamentoTotal = p.OrcamentoTotal,
                NumServicos = p.PedidoServicos.Count
            }).ToList();

            return Ok(new ClienteDetalheDto
            {
                Id = cliente.Id,
                NomeCompleto = cliente.NomeCompleto,
                Email = cliente.Email ?? string.Empty,
                Telefone = cliente.PhoneNumber,
                DataRegisto = cliente.DataRegisto,
                Pedidos = pedidos
            });
        }

        // DELETE /api/clientes/{id}
        /// <summary>Elimina um cliente. Falha se tiver pedidos associados.</summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Eliminar(string id)
        {
            var cliente = await _userManager.FindByIdAsync(id);
            if (cliente == null) return NotFound();

            var temPedidos = await _context.Pedidos.AnyAsync(p => p.ClienteId == id);
            if (temPedidos)
                return Conflict(new { erro = "Não é possível eliminar um cliente com pedidos associados." });

            var resultado = await _userManager.DeleteAsync(cliente);
            if (!resultado.Succeeded)
                return StatusCode(500, new { erro = "Erro ao eliminar o cliente.", detalhes = resultado.Errors });

            return NoContent();
        }
    }

    // ── DTOs ──

    public class ClienteDto
    {
        public string Id { get; set; } = string.Empty;
        public string NomeCompleto { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime DataRegisto { get; set; }
        public int TotalPedidos { get; set; }
    }

    public class ClienteDetalheDto
    {
        public string Id { get; set; } = string.Empty;
        public string NomeCompleto { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Telefone { get; set; }
        public DateTime DataRegisto { get; set; }
        public List<PedidoResumoClienteDto> Pedidos { get; set; } = new();
    }

    public class PedidoResumoClienteDto
    {
        public int Id { get; set; }
        public string TituloProjeto { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public string EstadoNome { get; set; } = string.Empty;
        public DateTime DataPedido { get; set; }
        public decimal? OrcamentoTotal { get; set; }
        public int NumServicos { get; set; }
    }

    internal static class EstadoPedidoHelper
    {
        internal static string EstadoNome(EstadoPedido estado) => estado switch
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
}
