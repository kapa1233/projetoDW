using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebServicos.Data;
using WebServicos.Models;

namespace WebServicos.Controllers
{
    /// <summary>
    /// API REST para gestão de categorias de serviços.
    /// Leitura pública (autenticado); escrita reservada a administradores.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CategoriasController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CategoriasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET /api/categorias
        /// <summary>Lista todas as categorias de serviços, com o total de serviços em cada uma.</summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CategoriaDto>>> Listar()
        {
            var categorias = await _context.CategoriasServico
                .Include(c => c.Servicos)
                .OrderBy(c => c.Nome)
                .Select(c => new CategoriaDto
                {
                    Id = c.Id,
                    Nome = c.Nome,
                    Descricao = c.Descricao,
                    TotalServicos = c.Servicos.Count,
                    ServicosAtivos = c.Servicos.Count(s => s.Ativo)
                })
                .ToListAsync();

            return Ok(categorias);
        }

        // GET /api/categorias/{id}
        /// <summary>Detalhe de uma categoria, incluindo os serviços que lhe pertencem.</summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<CategoriaDetalheDto>> Detalhe(int id)
        {
            var categoria = await _context.CategoriasServico
                .Include(c => c.Servicos)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (categoria == null) return NotFound();

            return Ok(new CategoriaDetalheDto
            {
                Id = categoria.Id,
                Nome = categoria.Nome,
                Descricao = categoria.Descricao,
                Servicos = categoria.Servicos.Select(s => new ServicoResumoDto
                {
                    Id = s.Id,
                    Nome = s.Nome,
                    PrecoBase = s.PrecoBase,
                    Ativo = s.Ativo,
                    Icone = s.Icone
                }).ToList()
            });
        }

        // POST /api/categorias
        /// <summary>Cria uma nova categoria. Apenas administradores.</summary>
        [HttpPost]
        [Authorize(Policy = "Administrador")]
        public async Task<ActionResult<CategoriaDto>> Criar([FromBody] CriarCategoriaDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Nome))
                return BadRequest(new { erro = "O nome da categoria é obrigatório." });

            var nomeJaExiste = await _context.CategoriasServico
                .AnyAsync(c => c.Nome.ToLower() == dto.Nome.ToLower().Trim());

            if (nomeJaExiste)
                return Conflict(new { erro = "Já existe uma categoria com esse nome." });

            var categoria = new CategoriaServico
            {
                Nome = dto.Nome.Trim(),
                Descricao = dto.Descricao?.Trim()
            };

            _context.CategoriasServico.Add(categoria);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(Detalhe), new { id = categoria.Id }, new CategoriaDto
            {
                Id = categoria.Id,
                Nome = categoria.Nome,
                Descricao = categoria.Descricao,
                TotalServicos = 0,
                ServicosAtivos = 0
            });
        }

        // PUT /api/categorias/{id}
        /// <summary>Atualiza uma categoria existente. Apenas administradores.</summary>
        [HttpPut("{id}")]
        [Authorize(Policy = "Administrador")]
        public async Task<IActionResult> Atualizar(int id, [FromBody] CriarCategoriaDto dto)
        {
            var categoria = await _context.CategoriasServico.FindAsync(id);
            if (categoria == null) return NotFound();

            if (string.IsNullOrWhiteSpace(dto.Nome))
                return BadRequest(new { erro = "O nome da categoria é obrigatório." });

            var nomeJaExiste = await _context.CategoriasServico
                .AnyAsync(c => c.Nome.ToLower() == dto.Nome.ToLower().Trim() && c.Id != id);

            if (nomeJaExiste)
                return Conflict(new { erro = "Já existe outra categoria com esse nome." });

            categoria.Nome = dto.Nome.Trim();
            categoria.Descricao = dto.Descricao?.Trim();

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE /api/categorias/{id}
        /// <summary>Elimina uma categoria. Apenas administradores. Os serviços ficam sem categoria.</summary>
        [HttpDelete("{id}")]
        [Authorize(Policy = "Administrador")]
        public async Task<IActionResult> Eliminar(int id)
        {
            var categoria = await _context.CategoriasServico.FindAsync(id);
            if (categoria == null) return NotFound();

            _context.CategoriasServico.Remove(categoria);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }

    // ── DTOs ──

    public class CategoriaDto
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string? Descricao { get; set; }
        public int TotalServicos { get; set; }
        public int ServicosAtivos { get; set; }
    }

    public class CategoriaDetalheDto
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string? Descricao { get; set; }
        public List<ServicoResumoDto> Servicos { get; set; } = new();
    }

    public class ServicoResumoDto
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public decimal PrecoBase { get; set; }
        public bool Ativo { get; set; }
        public string Icone { get; set; } = string.Empty;
    }

    public class CriarCategoriaDto
    {
        public string Nome { get; set; } = string.Empty;
        public string? Descricao { get; set; }
    }
}
