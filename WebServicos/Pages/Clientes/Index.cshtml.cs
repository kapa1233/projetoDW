using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebServicos.Data;
using WebServicos.Models;

namespace WebServicos.Pages.Clientes
{
    [Authorize(Roles = "Administrador")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public IndexModel(ApplicationDbContext db, UserManager<ApplicationUser> um)
        {
            _db = db;
            _userManager = um;
        }

        public List<ClienteViewModel> Clientes { get; set; } = new();

        public async Task OnGetAsync()
        {
            // LINQ com Join implícito via Include e projeção
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

    public class ClienteViewModel
    {
        public string Id { get; set; } = "";
        public string NomeCompleto { get; set; } = "";
        public string Email { get; set; } = "";
        public DateTime DataRegisto { get; set; }
        public int TotalPedidos { get; set; }
        public bool IsAdmin { get; set; }
    }
}
