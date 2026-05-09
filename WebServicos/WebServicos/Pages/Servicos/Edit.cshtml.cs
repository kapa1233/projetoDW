using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebServicos.Data;
using WebServicos.Models;

namespace WebServicos.Pages.Servicos
{
    /// <summary>
    /// Página de edição de um serviço existente. Exclusiva para administradores.
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

        /// <summary>Serviço a editar, ligado ao formulário via model binding.</summary>
        [BindProperty]
        public Servico Servico { get; set; } = null!;

        /// <summary>Carrega o serviço pelo ID para preencher o formulário de edição.</summary>
        public async Task<IActionResult> OnGetAsync(int id)
        {
            var servico = await _db.Servicos.FindAsync(id);
            if (servico == null)
                return NotFound();

            Servico = servico;
            return Page();
        }

        /// <summary>
        /// Atualiza os campos editáveis do serviço. Preserva DataCriacao para não sobrescrever.
        /// </summary>
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var servicoExistente = await _db.Servicos.FindAsync(Servico.Id);
            if (servicoExistente == null)
                return NotFound();

            try
            {
                // Atualizar apenas os campos editáveis (evita sobrescrever DataCriacao)
                servicoExistente.Nome = Servico.Nome;
                servicoExistente.Descricao = Servico.Descricao;
                servicoExistente.PrecoBase = Servico.PrecoBase;
                servicoExistente.Icone = Servico.Icone;
                servicoExistente.Ativo = Servico.Ativo;

                await _db.SaveChangesAsync();
                _logger.LogInformation("Serviço #{Id} atualizado.", Servico.Id);
                TempData["Sucesso"] = "Serviço atualizado com sucesso!";
                return RedirectToPage("Index");
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Erro de concorrência ao editar serviço #{Id}.", Servico.Id);
                ModelState.AddModelError(string.Empty, "O registo foi modificado por outro utilizador. Por favor recarregue.");
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao editar serviço.");
                ModelState.AddModelError(string.Empty, "Ocorreu um erro ao guardar.");
                return Page();
            }
        }
    }
}
