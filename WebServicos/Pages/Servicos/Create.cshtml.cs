using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebServicos.Data;
using WebServicos.Models;

namespace WebServicos.Pages.Servicos
{
    [Authorize(Roles = "Administrador")]
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<CreateModel> _logger;

        public CreateModel(ApplicationDbContext db, ILogger<CreateModel> logger)
        {
            _db = db;
            _logger = logger;
        }

        [BindProperty]
        public Servico Servico { get; set; } = new Servico();

        public IActionResult OnGet()
        {
            return Page();
        }

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
