using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebServicos.Data;
using WebServicos.Models;

namespace WebServicos.Pages.Pedidos
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public IndexModel(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public List<Pedido> Pedidos { get; set; } = new();

        public async Task OnGetAsync()
        {
            var userId = _userManager.GetUserId(User);

            var query = _db.Pedidos
                .Include(p => p.Cliente)
                .Include(p => p.PedidoServicos)
                .AsQueryable();

            // Admin vê tudo; cliente vê só os seus
            if (!User.IsInRole("Administrador"))
                query = query.Where(p => p.ClienteId == userId);

            Pedidos = await query
                .OrderByDescending(p => p.DataPedido)
                .ToListAsync();
        }
    }

    // ── Extension method para obter o DisplayName do enum ──
    public static class EstadoPedidoExtensions
    {
        public static string GetDisplayName(this EstadoPedido estado)
        {
            return estado switch
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
}
