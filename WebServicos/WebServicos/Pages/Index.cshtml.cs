using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebServicos.Data;
using WebServicos.Models;

namespace WebServicos.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _db;

        public IndexModel(ApplicationDbContext db)
        {
            _db = db;
        }

        public int TotalPedidosConcluidos { get; set; }
        public int TotalClientes { get; set; }
        public int TotalServicos { get; set; }
        public List<Servico> ServicosDestaque { get; set; } = new();

        public async Task OnGetAsync()
        {
            // Estatísticas — uso de LINQ
            TotalPedidosConcluidos = await _db.Pedidos
                .Where(p => p.Estado == EstadoPedido.Concluido)
                .CountAsync();

            TotalClientes = await _db.Users.CountAsync();
            TotalServicos = await _db.Servicos.Where(s => s.Ativo).CountAsync();

            // Três serviços em destaque — OrderBy feito no cliente (decimal não suportado pelo SQLite)
            ServicosDestaque = (await _db.Servicos
                .Where(s => s.Ativo)
                .ToListAsync())
                .OrderBy(s => s.PrecoBase)
                .Take(6)
                .ToList();
        }
    }
}
