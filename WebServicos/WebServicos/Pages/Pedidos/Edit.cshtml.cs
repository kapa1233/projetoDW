using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebServicos.Data;
using WebServicos.Models;

namespace WebServicos.Pages.Pedidos
{
    /// <summary>
    /// Página de edição de um pedido existente.
    /// Clientes propõem alterações (sujeitas a aprovação); administradores editam diretamente.
    /// </summary>
    [Authorize]
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<EditModel> _logger;

        /// <summary>Construtor com injeção de dependências.</summary>
        public EditModel(ApplicationDbContext db, UserManager<ApplicationUser> um, ILogger<EditModel> logger)
        {
            _db = db;
            _userManager = um;
            _logger = logger;
        }

        /// <summary>Pedido a editar, ligado ao formulário via model binding.</summary>
        [BindProperty]
        public Pedido Pedido { get; set; } = null!;

        /// <summary>URL do projeto entregue, só usado pelo admin ao marcar como Concluído.</summary>
        [BindProperty]
        public string? NovoEnderecoHttp { get; set; }

        /// <summary>Proposta de alteração pendente mais recente do cliente (se existir).</summary>
        public PedidoAlteracao? AlteracaoPendente { get; set; }

        /// <summary>
        /// Carrega o pedido para edição. Verifica autorização e restrições de estado.
        /// </summary>
        public async Task<IActionResult> OnGetAsync(int id)
        {
            var pedido = await _db.Pedidos.FindAsync(id);
            if (pedido == null) return NotFound();

            var userId = _userManager.GetUserId(User);
            var isAdmin = User.IsInRole("Administrador");

            // Apenas o cliente ou admin podem ver
            if (!isAdmin && pedido.ClienteId != userId)
                return Forbid();

            // Cliente só pode editar se pedido está Pendente
            if (!isAdmin && pedido.Estado != EstadoPedido.Pendente)
            {
                TempData["Erro"] = "Só é possível editar pedidos no estado Pendente.";
                return RedirectToPage("Index");
            }

            // Carrega a alteração pendente mais recente (se existir)
            AlteracaoPendente = await _db.PedidoAlteracoes
                .Where(a => a.PedidoId == id && a.Estado == EstadoAlteracao.Pendente)
                .OrderByDescending(a => a.DataPropostas)
                .FirstOrDefaultAsync();

            Pedido = pedido;
            return Page();
        }

        /// <summary>
        /// Processa a submissão do formulário. Clientes criam uma PedidoAlteracao para aprovação;
        /// administradores atualizam diretamente o estado e prazo.
        /// </summary>
        public async Task<IActionResult> OnPostAsync()
        {
            var pedidoExistente = await _db.Pedidos.FindAsync(Pedido.Id);
            if (pedidoExistente == null) return NotFound();

            var userId = _userManager.GetUserId(User);
            var isAdmin = User.IsInRole("Administrador");

            // Autorização
            if (!isAdmin && pedidoExistente.ClienteId != userId)
                return Forbid();

            try
            {
                // ── CLIENTE: Propõe alterações (não aplicadas direto) ──
                if (!isAdmin)
                {
                    if (string.IsNullOrWhiteSpace(Pedido.TituloProjeto))
                        ModelState.AddModelError("Pedido.TituloProjeto", "O título do projeto é obrigatório.");
                    if (string.IsNullOrWhiteSpace(Pedido.Descricao))
                        ModelState.AddModelError("Pedido.Descricao", "A descrição do pedido é obrigatória.");

                    if (!ModelState.IsValid)
                    {
                        Pedido = pedidoExistente;
                        return Page();
                    }

                    // Verifica se há alterações reais
                    bool houveAlteracoes = 
                        pedidoExistente.TituloProjeto != Pedido.TituloProjeto ||
                        pedidoExistente.Descricao != Pedido.Descricao ||
                        pedidoExistente.Observacoes != Pedido.Observacoes;

                    if (houveAlteracoes)
                    {
                        // Cria uma proposta de alteração pendente de aprovação
                        var alteracao = new PedidoAlteracao
                        {
                            PedidoId = Pedido.Id,
                            TituloProjetoProposto = Pedido.TituloProjeto,
                            DescricaoProposta = Pedido.Descricao,
                            ObservacoesProposta = Pedido.Observacoes,
                            DataPropostas = DateTime.UtcNow,
                            Estado = EstadoAlteracao.Pendente
                        };

                        _db.PedidoAlteracoes.Add(alteracao);
                        await _db.SaveChangesAsync();

                        _logger.LogInformation("Cliente {ClienteId} propôs alterações ao pedido #{Id}.", userId, Pedido.Id);
                        TempData["Sucesso"] = "Alterações propostas! Aguardando aprovação do administrador.";
                    }
                    else
                    {
                        TempData["Aviso"] = "Nenhuma alteração foi feita.";
                    }

                    return RedirectToPage("Index");
                }

                // ── ADMINISTRADOR: edita apenas PrazoEstimado e Estado ──
                else
                {
                    if (!ModelState.IsValid)
                    {
                        ModelState.Clear(); // Ignora erros de validação de outros campos
                    }

                    pedidoExistente.PrazoEstimado = Pedido.PrazoEstimado;
                    pedidoExistente.Estado = Pedido.Estado;

                    // Se está marcando como concluído, pode adicionar o URL
                    if (Pedido.Estado == EstadoPedido.Concluido && !string.IsNullOrWhiteSpace(NovoEnderecoHttp))
                    {
                        if (Uri.TryCreate(NovoEnderecoHttp, UriKind.Absolute, out _))
                        {
                            pedidoExistente.EnderecoHttp = NovoEnderecoHttp;
                            _logger.LogInformation("Admin adicionou URL ao pedido #{Id}: {Url}", Pedido.Id, NovoEnderecoHttp);
                        }
                        else
                        {
                            ModelState.AddModelError("NovoEnderecoHttp", "O endereço deve ser uma URL válida.");
                            Pedido = pedidoExistente;
                            return Page();
                        }
                    }

                    int rowsAffected = await _db.SaveChangesAsync();
                    _logger.LogInformation("Admin alterou o pedido #{Id}. Linhas afetadas: {RowsAffected}.", 
                        Pedido.Id, rowsAffected);

                    TempData["Sucesso"] = "Pedido atualizado com sucesso.";
                }

                return RedirectToPage("Index");
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Erro de base de dados ao atualizar pedido #{Id}.", Pedido.Id);
                ModelState.AddModelError(string.Empty, "Erro ao guardar as alterações na base de dados.");
                Pedido = pedidoExistente;
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado ao editar pedido #{Id}.", Pedido.Id);
                ModelState.AddModelError(string.Empty, "Erro ao guardar as alterações.");
                Pedido = pedidoExistente;
                return Page();
            }
        }
    }
}
