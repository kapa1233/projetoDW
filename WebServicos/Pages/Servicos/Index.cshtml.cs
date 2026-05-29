using Microsoft.EntityFrameworkCore;
using WebServicos.Data;
using WebServicos.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebServicos.Pages.Servicos
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _db;

        public IndexModel(ApplicationDbContext db)
        {
            _db = db;
        }

        public List<Servico> Servicos { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? Pesquisa { get; set; }

        public async Task OnGetAsync()
        {
            // LINQ com filtragem opcional por texto
            var query = _db.Servicos.AsQueryable();

            if (!string.IsNullOrWhiteSpace(Pesquisa))
            {
                query = query.Where(s =>
                    s.Nome.Contains(Pesquisa) ||
                    s.Descricao.Contains(Pesquisa));
            }

            Servicos = await query
                .Where(s => s.Ativo || User.IsInRole("Administrador"))
                .OrderBy(s => s.Nome)
                .ToListAsync();
        }
    }
}
