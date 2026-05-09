using Microsoft.EntityFrameworkCore;
using WebServicos.Data;
using WebServicos.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebServicos.Pages.Servicos
{
    /// <summary>
    /// Página pública de listagem de serviços disponíveis.
    /// Clientes veem apenas os serviços ativos; administradores veem todos.
    /// Suporta pesquisa por nome ou descrição via query string (?Pesquisa=xxx).
    /// </summary>
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _db;

        /// <summary>Construtor com injeção do contexto da base de dados.</summary>
        public IndexModel(ApplicationDbContext db)
        {
            _db = db;
        }

        /// <summary>Lista de serviços a apresentar na página.</summary>
        public List<Servico> Servicos { get; set; } = new();

        /// <summary>Texto de pesquisa introduzido pelo utilizador (via GET ?Pesquisa=...).</summary>
        [BindProperty(SupportsGet = true)]
        public string? Pesquisa { get; set; }

        /// <summary>
        /// Carrega a lista de serviços com filtro opcional por texto de pesquisa.
        /// Administradores veem também os serviços inativos.
        /// </summary>
        public async Task OnGetAsync()
        {
            // Construir query com filtro de pesquisa opcional
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
