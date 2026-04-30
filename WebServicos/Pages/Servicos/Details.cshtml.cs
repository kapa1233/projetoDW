using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebServicos.Data;
using WebServicos.Models;

namespace WebServicos.Pages.Servicos
{
    public class DetailsModel : PageModel
    {
        private readonly ApplicationDbContext _db;

        public DetailsModel(ApplicationDbContext db) => _db = db;

        public Servico? Servico { get; set; }
        public int TotalPedidos { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Servico = await _db.Servicos.FindAsync(id);
            if (Servico == null)
                return NotFound();

            // Contar quantos pedidos incluem este serviço (LINQ)
            TotalPedidos = await _db.PedidoServicos
                .Where(ps => ps.ServicoId == id)
                .CountAsync();

            return Page();
        }
    }
}
