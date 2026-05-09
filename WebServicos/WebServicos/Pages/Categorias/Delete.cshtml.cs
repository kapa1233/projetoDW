using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebServicos.Data;
using WebServicos.Models;

namespace WebServicos.Pages.Categorias
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

        public CategoriaServico Categoria { get; set; } = null!;
        public int NumServicos { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var cat = await _db.CategoriasServico
                .Include(c => c.Servicos)
                .FirstOrDefaultAsync(c => c.Id == id);
            if (cat == null) return NotFound();
            Categoria = cat;
            NumServicos = cat.Servicos.Count;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            var cat = await _db.CategoriasServico
                .Include(c => c.Servicos)
                .FirstOrDefaultAsync(c => c.Id == id);
            if (cat == null) return NotFound();

            try
            {
                _db.CategoriasServico.Remove(cat);
                await _db.SaveChangesAsync();
                _logger.LogInformation("Categoria '{Nome}' eliminada.", cat.Nome);
                TempData["Sucesso"] = $"Categoria \"{cat.Nome}\" eliminada. Os serviços associados ficaram sem categoria.";
                return RedirectToPage("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao eliminar categoria #{Id}.", id);
                TempData["Erro"] = "Ocorreu um erro ao eliminar a categoria.";
                return RedirectToPage("Index");
            }
        }
    }
}
