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

        /// <summary>Construtor com injeção de dependências.</summary>
        public ChatModel(ApplicationDbContext db, UserManager<ApplicationUser> um, ILogger<ChatModel> logger)
        {
            _db = db;
            _userManager = um;
            _logger = logger;
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

                NovaMsg = string.Empty;
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
