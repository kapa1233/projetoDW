using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebServicos.Data;
using WebServicos.Models;

namespace WebServicos.Pages.Categorias
{
    /// <summary>
    /// Página de edição de uma categoria de serviços existente. Exclusiva para administradores.
    /// </summary>
    [Authorize(Roles = "Administrador")]
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<EditModel> _logger;

        /// <summary>Construtor com injeção de dependências.</summary>
        public EditModel(ApplicationDbContext db, ILogger<EditModel> logger)
        {
            _db = db;
            _logger = logger;
        }

        /// <summary>Categoria a editar, ligada ao formulário via model binding.</summary>
        [BindProperty]
        public CategoriaServico Categoria { get; set; } = null!;

        /// <summary>Carrega a categoria pelo ID para preencher o formulário de edição.</summary>
        public async Task<IActionResult> OnGetAsync(int id)
        {
            var cat = await _db.CategoriasServico.FindAsync(id);
            if (cat == null) return NotFound();
            Categoria = cat;
            return Page();
        }

        /// <summary>
        /// Atualiza os campos editáveis da categoria (Nome e Descrição).
        /// Preserva os serviços associados sem os alterar.
        /// </summary>
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var existente = await _db.CategoriasServico.FindAsync(Categoria.Id);
            if (existente == null) return NotFound();

            try
            {
                // Atualizar apenas os campos editáveis (evita sobrescrever a lista de serviços)
                existente.Nome = Categoria.Nome;
                existente.Descricao = Categoria.Descricao;
                await _db.SaveChangesAsync();
                _logger.LogInformation("Categoria #{Id} atualizada.", Categoria.Id);
                TempData["Sucesso"] = "Categoria atualizada com sucesso!";
                return RedirectToPage("Index");
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Erro de concorrência ao editar categoria #{Id}.", Categoria.Id);
                ModelState.AddModelError(string.Empty, "O registo foi modificado por outro utilizador. Por favor recarregue.");
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao editar categoria.");
                ModelState.AddModelError(string.Empty, "Ocorreu um erro ao guardar.");
                return Page();
            }
        }
    }
}
