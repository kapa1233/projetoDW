using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebServicos.Data;
using WebServicos.Models;

namespace WebServicos.Pages.Servicos
{
    /// <summary>
    /// Página de criação de um novo serviço. Exclusiva para administradores.
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

        /// <summary>Dados do novo serviço, ligado ao formulário via model binding.</summary>
        [BindProperty]
        public Servico Servico { get; set; } = new Servico();

        /// <summary>Apresenta o formulário de criação.</summary>
        public IActionResult OnGet()
        {
            return Page();
        }

        /// <summary>Valida e guarda o novo serviço na base de dados.</summary>
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            try
            {
                Servico.DataCriacao = DateTime.UtcNow;
                _db.Servicos.Add(Servico);
                await _db.SaveChangesAsync();

                _logger.LogInformation("Serviço '{Nome}' criado com sucesso.", Servico.Nome);
                TempData["Sucesso"] = $"Serviço \"{Servico.Nome}\" criado com sucesso!";
                return RedirectToPage("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar serviço.");
                ModelState.AddModelError(string.Empty, "Ocorreu um erro ao guardar. Por favor tente novamente.");
                return Page();
            }
        }
    }
}
