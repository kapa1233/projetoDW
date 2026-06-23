using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using WebServicos.Data;
using WebServicos.Hubs;
using WebServicos.Models;
using WebServicos.Services;

namespace WebServicos.Pages.Pedidos
{
    /// <summary>
    /// Página de chat associado a um pedido.
    /// Permite comunicação em tempo real entre o cliente e o administrador.
    /// Ao carregar, marca automaticamente as mensagens não lidas como lidas.
    /// </summary>
    [Authorize]
    public class ChatModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<ChatModel> _logger;
        private readonly IHubContext<NotificacoesHub> _hubContext;
        private readonly INotificacaoService _notificacaoService;

        /// <summary>Construtor com injeção de dependências.</summary>
        public ChatModel(ApplicationDbContext db, UserManager<ApplicationUser> um, ILogger<ChatModel> logger, IHubContext<NotificacoesHub> hubContext, INotificacaoService notificacaoService)
        {
            _db = db;
            _userManager = um;
            _logger = logger;
            _hubContext = hubContext;
            _notificacaoService = notificacaoService;
        }

        /// <summary>Pedido ao qual este chat pertence.</summary>
        public Pedido? Pedido { get; set; }

        /// <summary>Lista de mensagens ordenadas cronologicamente.</summary>
        public List<Mensagem> Mensagens { get; set; } = new();

        /// <summary>Texto da nova mensagem a enviar, ligado ao formulário via model binding.</summary>
        [BindProperty]
        public string? NovaMsg { get; set; }

        /// <summary>
        /// Carrega o pedido e as mensagens do chat. Marca as mensagens não lidas como lidas.
        /// </summary>
        public async Task<IActionResult> OnGetAsync(int id)
        {
            var pedido = await _db.Pedidos
                .Include(p => p.Cliente)
                .Include(p => p.Mensagens)
                    .ThenInclude(m => m.Remetente)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (pedido == null) return NotFound();

            var userId = _userManager.GetUserId(User);
            var isAdmin = User.IsInRole("Administrador");

            // Apenas o cliente ou admin podem ver o chat
            if (!isAdmin && pedido.ClienteId != userId)
                return Forbid();

            Pedido = pedido;
            Mensagens = pedido.Mensagens.OrderBy(m => m.DataHora).ToList();

            // Marcar mensagens como lidas
            foreach (var msg in Mensagens.Where(m => !m.Lida && m.RemetenteId != userId))
            {
                msg.Lida = true;
            }

            if (Mensagens.Any(m => !m.Lida))
            {
                await _db.SaveChangesAsync();
            }

            return Page();
        }

        /// <summary>
        /// Guarda a nova mensagem na base de dados e redireciona para a mesma página.
        /// </summary>
        public async Task<IActionResult> OnPostAsync(int id)
        {
            if (string.IsNullOrWhiteSpace(NovaMsg))
            {
                ModelState.AddModelError("NovaMsg", "A mensagem não pode estar vazia.");
                return await OnGetAsync(id);
            }

            var pedido = await _db.Pedidos.FindAsync(id);
            if (pedido == null) return NotFound();

            var userId = _userManager.GetUserId(User);
            var isAdmin = User.IsInRole("Administrador");

            // Autorização
            if (!isAdmin && pedido.ClienteId != userId)
                return Forbid();

            try
            {
                var mensagem = new Mensagem
                {
                    PedidoId = id,
                    RemetenteId = userId,
                    Conteudo = NovaMsg.Trim(),
                    DataHora = DateTime.UtcNow,
                    Lida = false
                };

                _db.Mensagens.Add(mensagem);
                await _db.SaveChangesAsync();

                _logger.LogInformation("Mensagem adicionada ao pedido #{Id} por {UserId}.", id, userId);

                // Carrega os dados completos do remetente e pedido para a notificação
                var remetente = await _userManager.FindByIdAsync(userId!);
                var pedidoCompleto = await _db.Pedidos.FindAsync(id);

                if (remetente == null)
                {
                    _logger.LogWarning("Remetente com ID {UserId} não encontrado.", userId);
                }
                if (pedidoCompleto == null)
                {
                    _logger.LogWarning("Pedido #{Id} não encontrado para notificação.", id);
                }

                // Determinar destinatário(s) da notificação antes de qualquer chamada async
                var clienteIdNotif = pedido.ClienteId;
                List<string> adminIds = new();
                if (!isAdmin)
                {
                    var admins = await _userManager.GetUsersInRoleAsync("Administrador");
                    adminIds = admins.Select(a => a.Id).ToList();
                }

                // ✨ Persistir notificação no BD via serviço
                try
                {
                    var pedidoIdNotif = pedidoCompleto?.Id ?? id;
                    var remetenteNomeNotif = remetente?.NomeCompleto ?? "Utilizador Desconhecido";

                    if (isAdmin)
                    {
                        // Admin enviou → notifica o cliente
                        await _notificacaoService.NotificarNovaMensagemAsync(
                            pedidoIdNotif, remetenteNomeNotif, NovaMsg, clienteIdNotif);
                    }
                    else
                    {
                        // Cliente enviou → notifica todos os admins
                        foreach (var adminId in adminIds)
                        {
                            await _notificacaoService.NotificarNovaMensagemAsync(
                                pedidoIdNotif, remetenteNomeNotif, NovaMsg, adminId);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao guardar notificação de mensagem para pedido #{Id}.", id);
                }

                // Captura valores para o Task.Run (evita uso de serviços scoped em background)
                var pedidoIdBg = pedidoCompleto?.Id ?? id;
                var remetenteNomeBg = remetente?.NomeCompleto ?? "Utilizador Desconhecido";
                var tipoRemetenteBg = isAdmin ? "Admin" : "Cliente";
                var horaBg = DateTime.UtcNow.ToString("HH:mm");
                var adminIdsBg = new List<string>(adminIds);

                // Dispara evento SignalR em background (sem esperar)
                _ = Task.Run(async () =>
                {
                    try
                    {
                        if (isAdmin)
                        {
                            // Admin enviou mensagem → notifica só o cliente
                            await _hubContext.Clients.User(clienteIdNotif).SendAsync(
                                "MensagemEnviada", pedidoIdBg, remetenteNomeBg, tipoRemetenteBg, horaBg);
                        }
                        else
                        {
                            // Cliente enviou mensagem → notifica cada admin
                            foreach (var adminId in adminIdsBg)
                            {
                                await _hubContext.Clients.User(adminId).SendAsync(
                                    "MensagemEnviada", pedidoIdBg, remetenteNomeBg, tipoRemetenteBg, horaBg);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Erro ao enviar notificação SignalR do pedido #{Id}.", pedidoIdBg);
                    }
                });

                NovaMsg = string.Empty;
                
                // Aguarda um pouco para garantir que a notificação é enviada antes do redirect
                await Task.Delay(200);
                
                return RedirectToPage(new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao guardar mensagem no pedido #{Id}.", id);
                ModelState.AddModelError(string.Empty, "Erro ao enviar mensagem.");
                return await OnGetAsync(id);
            }
        }
    }
}
