using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebServicos.Data;
using WebServicos.Models;

namespace WebServicos.Pages.Categorias
{
    /// <summary>
    /// Página de criação de uma nova categoria de serviços. Exclusiva para administradores.
    /// </summary>
    [Authorize(Roles = "Administrador")]
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<CreateModel> _logger;

        /// <summary>Construtor com injeção de dependências.</summary>
        public CreateModel(ApplicationDbContext db, ILogger<CreateModel> logger)
        {
            _db = db;
            _logger = logger;
        }

        /// <summary>Dados da nova categoria, ligado ao formulário via model binding.</summary>
        [BindProperty]
        public CategoriaServico Categoria { get; set; } = new();

        /// <summary>Apresenta o formulário de criação.</summary>
        public IActionResult OnGet() => Page();

        /// <summary>Valida e guarda a nova categoria na base de dados.</summary>
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            try
            {
                _db.CategoriasServico.Add(Categoria);
                await _db.SaveChangesAsync();
                _logger.LogInformation("Categoria '{Nome}' criada.", Categoria.Nome);
                TempData["Sucesso"] = $"Categoria \"{Categoria.Nome}\" criada com sucesso!";
                return RedirectToPage("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar categoria.");
                ModelState.AddModelError(string.Empty, "Ocorreu um erro ao guardar. Por favor tente novamente.");
                return Page();
            }
        }
    }
}
