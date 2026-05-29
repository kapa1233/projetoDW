using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using WebServicos.Data;
using WebServicos.Hubs;
using WebServicos.Models;

namespace WebServicos.Pages.Pedidos
{
    /// <summary>
    /// Página de criação de um novo pedido de serviço.
    /// Apenas acessível a clientes; administradores são redirecionados.
    /// </summary>
    [Authorize]
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHubContext<NotificacoesHub> _hub;
        private readonly ILogger<CreateModel> _logger;

        /// <summary>Construtor com injeção de todas as dependências necessárias.</summary>
        public CreateModel(
            ApplicationDbContext db,
            UserManager<ApplicationUser> userManager,
            IHubContext<NotificacoesHub> hub,
            ILogger<CreateModel> logger)
        {
            _db = db;
            _userManager = userManager;
            _hub = hub;
            _logger = logger;
        }

        /// <summary>Dados do novo pedido, ligado ao formulário via model binding.</summary>
        [BindProperty]
        public Pedido Pedido { get; set; } = new();

        /// <summary>IDs dos serviços selecionados pelo cliente no formulário.</summary>
        [BindProperty]
        [Required(ErrorMessage = "Selecione pelo menos um serviço.")]
        public List<int> ServicosIds { get; set; } = new();

        /// <summary>Lista de serviços disponíveis para apresentar no formulário.</summary>
        public List<Servico> ServicosDisponiveis { get; set; } = new();

        /// <summary>Mensagem de erro específica para a seleção de serviços.</summary>
        public string? ErroServicos { get; set; }

        /// <summary>
        /// Carrega a lista de serviços ativos. Aceita um servicoId opcional para pré-selecionar
        /// um serviço quando o utilizador clica "Contratar" na página de serviços.
        /// </summary>
        /// <param name="servicoId">ID do serviço a pré-selecionar (opcional).</param>
        public async Task<IActionResult> OnGetAsync(int? servicoId)
        {
            // ── Apenas clientes podem criar pedidos ──
            if (User.IsInRole("Administrador"))
            {
                TempData["Erro"] = "Apenas clientes podem criar pedidos.";
                return RedirectToPage("Index");
            }

            ServicosDisponiveis = await _db.Servicos.Where(s => s.Ativo).ToListAsync();

            // Pré-selecionar serviço se veio da página de detalhes
            if (servicoId.HasValue)
                ServicosIds.Add(servicoId.Value);

            return Page();
        }

        /// <summary>
        /// Processa o formulário de criação do pedido. Valida os dados, calcula o orçamento
        /// e persiste o pedido e os serviços numa transação para garantir consistência.
        /// </summary>
        public async Task<IActionResult> OnPostAsync()
        {
            // ── Apenas clientes podem criar pedidos ──
            if (User.IsInRole("Administrador"))
            {
                TempData["Erro"] = "Apenas clientes podem criar pedidos.";
                return RedirectToPage("Index");
            }

            // Carregar novamente os serviços para repopular a página em caso de erro
            ServicosDisponiveis = await _db.Servicos.Where(s => s.Ativo).ToListAsync();

            // Validação simples: pelo menos um serviço selecionado
            if (ServicosIds is null || !ServicosIds.Any())
            {
                ModelState.AddModelError("ServicosIds", "Por favor selecione pelo menos um serviço.");
            }

            // Validação de prazo mínimo (opcional)
            if (Pedido.PrazoEstimado.HasValue && Pedido.PrazoEstimado < DateTime.Today.AddDays(7))
            {
                ModelState.AddModelError(nameof(Pedido.PrazoEstimado), "O prazo deve ser pelo menos 7 dias a partir de hoje.");
            }

            // O ClienteId é definido no servidor (utilizador autenticado)
            ModelState.Remove("Pedido.ClienteId");

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            // Preparar entidade Pedido
            Pedido.ClienteId = user.Id;
            Pedido.DataPedido = DateTime.UtcNow;
            Pedido.Estado = EstadoPedido.Pendente;

            // Obter serviços selecionados e calcular orçamento
            var servicosSelecionados = await _db.Servicos
                .Where(s => ServicosIds.Contains(s.Id) && s.Ativo)
                .ToListAsync();
            if (servicosSelecionados.Count == 0)
            {
                ModelState.AddModelError("ServicosIds", "Os serviços selecionados já não estão disponíveis.");
                return Page();
            }
            Pedido.OrcamentoTotal = servicosSelecionados.Sum(s => s.PrecoBase);

            // Persistir de forma transacional
            await using var tx = await _db.Database.BeginTransactionAsync();
            try
            {
                _db.Pedidos.Add(Pedido);
                await _db.SaveChangesAsync();

                var ligacoes = servicosSelecionados.Select(s => new PedidoServico
                {
                    PedidoId = Pedido.Id,
                    ServicoId = s.Id,
                    PrecoAcordado = s.PrecoBase
                });
                await _db.PedidoServicos.AddRangeAsync(ligacoes);
                await _db.SaveChangesAsync();

                await tx.CommitAsync();
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                _logger.LogError(ex, "Erro ao guardar o pedido");
                ModelState.AddModelError(string.Empty, "Erro ao guardar o pedido. Por favor tente novamente.");
                return Page();
            }

            TempData["Sucesso"] = "Pedido enviado com sucesso! Entraremos em contacto brevemente.";
            return RedirectToPage("Index");
        }
    }
}
