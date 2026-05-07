using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebServicos.Data;
using WebServicos.Models;

namespace WebServicos.Pages.Servicos
{
    [Authorize(Roles = "Administrador")]
    public class DeleteModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<DeleteModel> _logger;

        public DeleteModel(ApplicationDbContext db, ILogger<DeleteModel> logger)
        {
            _db = db;
            _logger = logger;
        }

        [BindProperty]
        public Servico? Servico { get; set; }
        public int TotalPedidos { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Servico = await _db.Servicos.FindAsync(id);
            if (Servico == null)
                return NotFound();

            TotalPedidos = await _db.PedidoServicos.CountAsync(ps => ps.ServicoId == id);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var servico = await _db.Servicos.FindAsync(Servico!.Id);
            if (servico == null)
                return NotFound();

            // Verificar se há pedidos associados antes de eliminar
            var temPedidos = await _db.PedidoServicos.AnyAsync(ps => ps.ServicoId == servico.Id);
            if (temPedidos)
            {
                TempData["Erro"] = "Não é possível eliminar um serviço com pedidos associados.";
                return RedirectToPage("Index");
            }

            try
            {
                _db.Servicos.Remove(servico);
                await _db.SaveChangesAsync();
                _logger.LogInformation("Serviço '{Nome}' eliminado.", servico.Nome);
                TempData["Sucesso"] = $"Serviço \"{servico.Nome}\" eliminado com sucesso.";
                return RedirectToPage("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao eliminar serviço #{Id}.", servico.Id);
                TempData["Erro"] = "Erro ao eliminar o serviço. Por favor tente novamente.";
                return RedirectToPage("Index");
            }
        }
    }
}
