using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebServicos.Data;
using WebServicos.Models;

namespace WebServicos.Pages.Pedidos
{
    [Authorize(Roles = "Administrador")]
    public class RevisarAlteracoesModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<RevisarAlteracoesModel> _logger;

        public RevisarAlteracoesModel(ApplicationDbContext db, UserManager<ApplicationUser> um, ILogger<RevisarAlteracoesModel> logger)
        {
            _db = db;
            _userManager = um;
            _logger = logger;
        }

        public PedidoAlteracao? Alteracao { get; set; }
        public Pedido? Pedido { get; set; }
        public List<PedidoAlteracao> AlteracoesPendentes { get; set; } = new();

        [BindProperty]
        public string? MotivoRejeicao { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            // Se tem ID, mostra o detalhe de uma alteração específica
            if (id.HasValue)
            {
                Alteracao = await _db.PedidoAlteracoes
                    .Include(a => a.Pedido)
                        .ThenInclude(p => p.Cliente)
                    .FirstOrDefaultAsync(a => a.Id == id && a.Estado == EstadoAlteracao.Pendente);

                if (Alteracao == null) return NotFound();

                Pedido = Alteracao.Pedido;
                return Page();
            }

            // Se não tem ID, lista todas as alterações pendentes
            AlteracoesPendentes = await _db.PedidoAlteracoes
                .Include(a => a.Pedido)
                    .ThenInclude(p => p.Cliente)
                .Where(a => a.Estado == EstadoAlteracao.Pendente)
                .OrderByDescending(a => a.DataPropostas)
                .ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostAprovarAsync(int id)
        {
            var alteracao = await _db.PedidoAlteracoes
                .Include(a => a.Pedido)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (alteracao == null) return NotFound();

            var userId = _userManager.GetUserId(User);

            try
            {
                // Aplicar as alterações ao pedido
                alteracao.Pedido.TituloProjeto = alteracao.TituloProjetoProposto ?? alteracao.Pedido.TituloProjeto;
                alteracao.Pedido.Descricao = alteracao.DescricaoProposta ?? alteracao.Pedido.Descricao;
                alteracao.Pedido.Observacoes = alteracao.ObservacoesProposta ?? alteracao.Pedido.Observacoes;

                // Marcar a alteração como aprovada
                alteracao.Estado = EstadoAlteracao.Aprovado;
                alteracao.DataDecisao = DateTime.UtcNow;
                alteracao.DecididoPorId = userId;

                await _db.SaveChangesAsync();

                _logger.LogInformation("Admin {AdminId} aprovou alterações #{AlteracaoId} ao pedido #{PedidoId}.",
                    userId, id, alteracao.PedidoId);

                TempData["Sucesso"] = "Alterações aprovadas e aplicadas ao pedido.";
                return RedirectToPage("/Pedidos/RevisarAlteracoes", new { page = 1 });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao aprovar alterações #{AlteracaoId}.", id);
                ModelState.AddModelError(string.Empty, "Erro ao aprovar as alterações.");
                return await OnGetAsync(id);
            }
        }

        public async Task<IActionResult> OnPostRejeitarAsync(int id)
        {
            if (string.IsNullOrWhiteSpace(MotivoRejeicao))
            {
                ModelState.AddModelError("MotivoRejeicao", "Deve indicar o motivo da rejeição.");
                return await OnGetAsync(id);
            }

            var alteracao = await _db.PedidoAlteracoes
                .FirstOrDefaultAsync(a => a.Id == id);

            if (alteracao == null) return NotFound();

            var userId = _userManager.GetUserId(User);

            try
            {
                // Marcar a alteração como rejeitada
                alteracao.Estado = EstadoAlteracao.Rejeitado;
                alteracao.MotivoRejeicao = MotivoRejeicao.Trim();
                alteracao.DataDecisao = DateTime.UtcNow;
                alteracao.DecididoPorId = userId;

                await _db.SaveChangesAsync();

                _logger.LogInformation("Admin {AdminId} rejeitou alterações #{AlteracaoId} ao pedido #{PedidoId}.",
                    userId, id, alteracao.PedidoId);

                TempData["Sucesso"] = "Alterações rejeitadas.";
                return RedirectToPage("/Pedidos/RevisarAlteracoes", new { page = 1 });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao rejeitar alterações #{AlteracaoId}.", id);
                ModelState.AddModelError(string.Empty, "Erro ao rejeitar as alterações.");
                return await OnGetAsync(id);
            }
        }
    }
}
