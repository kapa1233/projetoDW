using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebServicos.Data;
using WebServicos.Models;

namespace WebServicos.Pages.Pedidos
{
    [Authorize]
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<EditModel> _logger;

        public EditModel(ApplicationDbContext db, UserManager<ApplicationUser> um, ILogger<EditModel> logger)
        {
            _db = db;
            _userManager = um;
            _logger = logger;
        }

        [BindProperty]
        public Pedido Pedido { get; set; } = null!;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var pedido = await _db.Pedidos.FindAsync(id);
            if (pedido == null) return NotFound();

            var userId = _userManager.GetUserId(User);
            if (!User.IsInRole("Administrador") && pedido.ClienteId != userId)
                return Forbid();

            if (!User.IsInRole("Administrador") && pedido.Estado != EstadoPedido.Pendente)
            {
                TempData["Erro"] = "Só é possível editar pedidos no estado Pendente.";
                return RedirectToPage("Index");
            }

            Pedido = pedido;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            var pedidoExistente = await _db.Pedidos.FindAsync(Pedido.Id);
            if (pedidoExistente == null) return NotFound();

            var userId = _userManager.GetUserId(User);
            if (!User.IsInRole("Administrador") && pedidoExistente.ClienteId != userId)
                return Forbid();

            try
            {
                pedidoExistente.TituloProjeto = Pedido.TituloProjeto;
                pedidoExistente.Descricao = Pedido.Descricao;
                pedidoExistente.PrazoEstimado = Pedido.PrazoEstimado;
                pedidoExistente.Observacoes = Pedido.Observacoes;

                // Apenas admins podem alterar o estado via edição
                if (User.IsInRole("Administrador"))
                    pedidoExistente.Estado = Pedido.Estado;

                await _db.SaveChangesAsync();
                _logger.LogInformation("Pedido #{Id} atualizado.", Pedido.Id);
                TempData["Sucesso"] = "Pedido atualizado com sucesso.";
                return RedirectToPage("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao editar pedido #{Id}.", Pedido.Id);
                ModelState.AddModelError(string.Empty, "Erro ao guardar as alterações.");
                return Page();
            }
        }
    }
}
