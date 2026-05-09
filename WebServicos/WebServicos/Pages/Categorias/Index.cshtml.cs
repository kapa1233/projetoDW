using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebServicos.Data;
using WebServicos.Models;

namespace WebServicos.Pages.Categorias
{
    /// <summary>
    /// Página de listagem de categorias de serviços. Exclusiva para administradores.
    /// Inclui o número de serviços em cada categoria via eager loading.
    /// </summary>
    [Authorize(Roles = "Administrador")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<IndexModel> _logger;

        /// <summary>Construtor com injeção de dependências.</summary>
        public IndexModel(ApplicationDbContext db, ILogger<IndexModel> logger)
        {
            _db = db;
            _logger = logger;
        }

        /// <summary>Lista de categorias com os serviços associados.</summary>
        public List<CategoriaServico> Categorias { get; set; } = new();

        /// <summary>Carrega todas as categorias com os serviços incluídos (eager loading).</summary>
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
