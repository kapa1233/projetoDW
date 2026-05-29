using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebServicos.Data;
using WebServicos.Models;

namespace WebServicos.Pages.Categorias
{
    /// <summary>
    /// Página de confirmação de eliminação de uma categoria. Exclusiva para administradores.
    /// Informa quantos serviços ficarão sem categoria após a eliminação.
    /// </summary>
    [Authorize(Roles = "Administrador")]
    public class DeleteModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<DeleteModel> _logger;

        /// <summary>Construtor com injeção de dependências.</summary>
        public DeleteModel(ApplicationDbContext db, ILogger<DeleteModel> logger)
        {
            _db = db;
            _logger = logger;
        }

        /// <summary>Categoria a eliminar, carregada para mostrar na página de confirmação.</summary>
        public CategoriaServico Categoria { get; set; } = null!;

        /// <summary>Número de serviços associados a esta categoria (ficam sem categoria após eliminação).</summary>
        public int NumServicos { get; set; }

        /// <summary>
        /// Carrega a categoria com os serviços associados via eager loading
        /// para mostrar o aviso de impacto antes da confirmação.
        /// </summary>
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

        /// <summary>
        /// Elimina a categoria. Os serviços associados ficam com CategoriaId = null
        /// graças à configuração de cascade definida no ApplicationDbContext.
        /// </summary>
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
