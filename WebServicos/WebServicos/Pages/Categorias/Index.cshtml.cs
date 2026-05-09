using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebServicos.Data;
using WebServicos.Models;

namespace WebServicos.Pages.Categorias
{
    [Authorize(Roles = "Administrador")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ApplicationDbContext db, ILogger<IndexModel> logger)
        {
            _db = db;
            _logger = logger;
        }

        public List<CategoriaServico> Categorias { get; set; } = new();

        public async Task OnGetAsync()
        {
            Categorias = await _db.CategoriasServico
                .Include(c => c.Servicos)
                .OrderBy(c => c.Nome)
                .ToListAsync();

            _logger.LogInformation("Listagem de {Total} categorias.", Categorias.Count);
        }
    }
}
