using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebServicos.Data;
using WebServicos.Models;

namespace WebServicos.Pages.Pedidos
{
    /// <summary>
    /// Página de listagem de pedidos.
    /// Administradores veem todos os pedidos; clientes veem apenas os seus.
    /// </summary>
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        /// <summary>Construtor com injeção do contexto da BD e gestor de utilizadores.</summary>
        public IndexModel(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        /// <summary>Lista de pedidos a apresentar na página.</summary>
        public List<Pedido> Pedidos { get; set; } = new();

        /// <summary>
        /// Carrega os pedidos. Admin vê todos; cliente vê apenas os seus.
        /// Inclui informação do cliente e dos serviços associados (eager loading).
        /// </summary>
        public async Task OnGetAsync()
        {
            var userId = _userManager.GetUserId(User);

            var query = _db.Pedidos
                .Include(p => p.Cliente)
                .Include(p => p.PedidoServicos)
                .AsQueryable();

            // Admin vê tudo; cliente vê só os seus
            if (!User.IsInRole("Administrador"))
                query = query.Where(p => p.ClienteId == userId);

            Pedidos = await query
                .OrderByDescending(p => p.DataPedido)
                .ToListAsync();
        }
    }

    /// <summary>
    /// Métodos de extensão para o enum EstadoPedido.
    /// </summary>
    public static class EstadoPedidoExtensions
    {
        /// <summary>
        /// Converte o valor do enum para o nome legível em português.
        /// </summary>
        public static string GetDisplayName(this EstadoPedido estado)
        {
            return estado switch
            {
                EstadoPedido.Pendente => "Pendente",
                EstadoPedido.EmAnalise => "Em Análise",
                EstadoPedido.Aprovado => "Aprovado",
                EstadoPedido.EmDesenvolvimento => "Em Desenvolvimento",
                EstadoPedido.EmRevisao => "Em Revisão",
                EstadoPedido.Concluido => "Concluído",
                EstadoPedido.Cancelado => "Cancelado",
                _ => estado.ToString()
            };
        }
    }
}
