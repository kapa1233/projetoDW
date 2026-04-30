using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using WebServicos.Data;
using WebServicos.Hubs;
using WebServicos.Models;

namespace WebServicos.Pages.Pedidos
{
    [Authorize]
    public class DetailsModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHubContext<NotificacoesHub> _hub;

        public DetailsModel(ApplicationDbContext db, UserManager<ApplicationUser> um, IHubContext<NotificacoesHub> hub)
        {
            _db = db;
            _userManager = um;
            _hub = hub;
        }

        public Pedido? Pedido { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Pedido = await _db.Pedidos
                .Include(p => p.Cliente)
                .Include(p => p.PedidoServicos)
                    .ThenInclude(ps => ps.Servico)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (Pedido == null) return NotFound();

            // Clientes só podem ver os seus próprios pedidos
            var userId = _userManager.GetUserId(User);
            if (!User.IsInRole("Administrador") && Pedido.ClienteId != userId)
                return Forbid();

            return Page();
        }

        // Handler para administrador alterar estado
        public async Task<IActionResult> OnPostAlterarEstadoAsync(int id, int novoEstado)
        {
            if (!User.IsInRole("Administrador"))
                return Forbid();

            var pedido = await _db.Pedidos.Include(p => p.Cliente).FirstOrDefaultAsync(p => p.Id == id);
            if (pedido == null) return NotFound();

            var estadoAnterior = pedido.Estado;
            pedido.Estado = (EstadoPedido)novoEstado;
            await _db.SaveChangesAsync();

            // Notificar cliente via SignalR
            await _hub.Clients.User(pedido.ClienteId).SendAsync(
                "PedidoAtualizado",
                pedido.TituloProjeto,
                pedido.Estado.GetDisplayName(),
                DateTime.Now.ToString("HH:mm"));

            TempData["Sucesso"] = "Estado do pedido atualizado com sucesso.";
            return RedirectToPage(new { id });
        }
    }
}
