using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebServicos.Data;
using WebServicos.Models;

namespace WebServicos.Pages.Clientes
{
    /// <summary>
    /// Página de gestão de clientes. Exclusiva para administradores.
    /// Lista todos os utilizadores com o total de pedidos e indica quais são administradores.
    /// </summary>
    [Authorize(Roles = "Administrador")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        /// <summary>Construtor com injeção de dependências.</summary>
        public IndexModel(ApplicationDbContext db, UserManager<ApplicationUser> um)
        {
            _db = db;
            _userManager = um;
        }

        /// <summary>Lista de clientes com os dados resumidos para apresentar na tabela.</summary>
        public List<ClienteViewModel> Clientes { get; set; } = new();

        /// <summary>
        /// Carrega todos os utilizadores com os seus pedidos e verifica o role de cada um.
        /// Usa eager loading para incluir os pedidos sem N+1 queries.
        /// </summary>
        public async Task OnGetAsync()
        {
            // LINQ com Include para carregar os pedidos de cada utilizador (evita N+1)
            var users = await _db.Users
                .Include(u => u.Pedidos)
                .OrderBy(u => u.NomeCompleto)
                .ToListAsync();

            foreach (var u in users)
            {
                Clientes.Add(new ClienteViewModel
                {
                    Id = u.Id,
                    NomeCompleto = u.NomeCompleto,
                    Email = u.Email ?? "",
                    DataRegisto = u.DataRegisto,
                    TotalPedidos = u.Pedidos.Count,
                    IsAdmin = await _userManager.IsInRoleAsync(u, DbSeeder.RoleAdmin)
                });
            }
        }
    }

    /// <summary>
    /// ViewModel para apresentar um utilizador na listagem de clientes.
    /// Agrega dados do utilizador com métricas calculadas.
    /// </summary>
    public class ClienteViewModel
    {
        /// <summary>ID único do utilizador (GUID do Identity).</summary>
        public string Id { get; set; } = "";

        /// <summary>Nome completo do utilizador.</summary>
        public string NomeCompleto { get; set; } = "";

        /// <summary>Endereço de email do utilizador.</summary>
        public string Email { get; set; } = "";

        /// <summary>Data em que o utilizador se registou na plataforma.</summary>
        public DateTime DataRegisto { get; set; }

        /// <summary>Número total de pedidos submetidos por este utilizador.</summary>
        public int TotalPedidos { get; set; }

        /// <summary>Indica se o utilizador tem o role de Administrador.</summary>
        public bool IsAdmin { get; set; }
    }
}
