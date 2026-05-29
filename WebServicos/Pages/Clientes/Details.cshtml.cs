using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebServicos.Data;
using WebServicos.Models;

namespace WebServicos.Pages.Clientes
{
    [Authorize(Roles = "Administrador")]
    public class DetailsModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public DetailsModel(ApplicationDbContext db, UserManager<ApplicationUser> um)
        {
            _db = db;
            _userManager = um;
        }

        public ApplicationUser? Cliente { get; set; }
        public List<Pedido> Pedidos { get; set; } = new();
        public bool IsAdmin { get; set; }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            Cliente = await _db.Users.FindAsync(id);
            if (Cliente == null) return NotFound();

            IsAdmin = await _userManager.IsInRoleAsync(Cliente, DbSeeder.RoleAdmin);

            Pedidos = await _db.Pedidos
                .Where(p => p.ClienteId == id)
                .OrderByDescending(p => p.DataPedido)
                .ToListAsync();

            return Page();
        }
    }
}
