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
    /// <summary>
    /// Página de detalhes de um pedido. Mostra todos os dados do pedido e os serviços incluídos.
    /// Administradores podem alterar o estado diretamente nesta página.
    /// </summary>
    [Authorize]
    public class DetailsModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHubContext<NotificacoesHub> _hub;

        /// <summary>Construtor com injeção de dependências.</summary>
        public DetailsModel(ApplicationDbContext db, UserManager<ApplicationUser> um, IHubContext<NotificacoesHub> hub)
        {
            _db = db;
            _userManager = um;
            _hub = hub;
        }

        /// <summary>Pedido a apresentar na página.</summary>
        public Pedido? Pedido { get; set; }

        /// <summary>Novo estado selecionado pelo administrador no formulário.</summary>
        [BindProperty]
        public EstadoPedido NovoEstado { get; set; }

        /// <summary>
        /// Carrega os detalhes do pedido com eager loading de cliente e serviços.
        /// Clientes só podem ver os seus próprios pedidos.
        /// </summary>
        public async Task<IActionResult> OnGetAsync(int id)
        {
            Pedido = await _db.Pedidos
                .Include(p => p.Cliente)
                .Include(p => p.PedidoServicos)
                    .ThenInclude(ps => ps.Servico)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (Pedido == null) return NotFound();

            NovoEstado = Pedido.Estado;

            // Clientes só podem ver os seus próprios pedidos
            var userId = _userManager.GetUserId(User);
            if (!User.IsInRole("Administrador") && Pedido.ClienteId != userId)
                return Forbid();

            return Page();
        }

        /// <summary>
        /// Handler POST para alterar o estado do pedido. Restrito a administradores.
        /// Após guardar, notifica o cliente via SignalR em tempo real.
        /// </summary>
        public async Task<IActionResult> OnPostAlterarEstadoAsync(int id)
        {
            if (!User.IsInRole("Administrador"))
                return Forbid();

            var pedido = await _db.Pedidos.Include(p => p.Cliente).FirstOrDefaultAsync(p => p.Id == id);
            if (pedido == null) return NotFound();

            var estadoAnterior = pedido.Estado;
            pedido.Estado = NovoEstado;
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
