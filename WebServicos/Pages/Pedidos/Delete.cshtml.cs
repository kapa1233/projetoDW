using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebServicos.Data;
using WebServicos.Models;

namespace WebServicos.Pages.Pedidos
{
    [Authorize]
    public class DeleteModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<DeleteModel> _logger;

        public DeleteModel(ApplicationDbContext db, UserManager<ApplicationUser> um, ILogger<DeleteModel> logger)
        {
            _db = db;
            _userManager = um;
            _logger = logger;
        }

        [BindProperty]
        public Pedido? Pedido { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Pedido = await _db.Pedidos.FindAsync(id);
            if (Pedido == null) return NotFound();

            var userId = _userManager.GetUserId(User);
            if (!User.IsInRole("Administrador") && Pedido.ClienteId != userId)
                return Forbid();

            if (Pedido.Estado != EstadoPedido.Pendente && !User.IsInRole("Administrador"))
            {
                TempData["Erro"] = "Apenas pedidos no estado Pendente podem ser cancelados.";
                return RedirectToPage("Index");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var pedido = await _db.Pedidos.FindAsync(Pedido!.Id);
            if (pedido == null) return NotFound();

            try
            {
                // Remover relações muitos-para-muitos primeiro
                var relacoes = _db.PedidoServicos.Where(ps => ps.PedidoId == pedido.Id);
                _db.PedidoServicos.RemoveRange(relacoes);
                _db.Pedidos.Remove(pedido);
                await _db.SaveChangesAsync();

                _logger.LogInformation("Pedido #{Id} eliminado.", pedido.Id);
                TempData["Sucesso"] = "Pedido cancelado com sucesso.";
                return RedirectToPage("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao eliminar pedido #{Id}.", pedido.Id);
                TempData["Erro"] = "Erro ao cancelar o pedido.";
                return RedirectToPage("Index");
            }
        }
    }
}
