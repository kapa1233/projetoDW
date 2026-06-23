using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebServicos.Data;
using WebServicos.Models;

namespace WebServicos.Controllers
{
    /// <summary>
    /// API REST para gestão de serviços.
    /// Leitura pública (autenticado); escrita reservada a administradores.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ServicosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ServicosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET /api/servicos
        /// <summary>Lista todos os serviços. Query param ?apenasAtivos=true filtra inativos.</summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ServicoDto>>> Listar([FromQuery] bool apenasAtivos = false)
        {
            var query = _context.Servicos
                .Include(s => s.CategoriaServico)
                .AsQueryable();

            if (apenasAtivos)
                query = query.Where(s => s.Ativo);

            var servicos = await query
                .OrderBy(s => s.CategoriaServicoId)
                .ThenBy(s => s.Nome)
                .Select(s => new ServicoDto
                {
                    Id = s.Id,
                    Nome = s.Nome,
                    Descricao = s.Descricao,
                    PrecoBase = s.PrecoBase,
                    Ativo = s.Ativo,
                    Icone = s.Icone,
                    DataCriacao = s.DataCriacao,
                    CategoriaId = s.CategoriaServicoId,
                    CategoriaNome = s.CategoriaServico != null ? s.CategoriaServico.Nome : null
                })
                .ToListAsync();

            return Ok(servicos);
        }

        // GET /api/servicos/{id}
        /// <summary>Detalhe de um serviço.</summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ServicoDto>> Detalhe(int id)
        {
            var servico = await _context.Servicos
                .Include(s => s.CategoriaServico)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (servico == null) return NotFound();

            return Ok(new ServicoDto
            {
                Id = servico.Id,
                Nome = servico.Nome,
                Descricao = servico.Descricao,
                PrecoBase = servico.PrecoBase,
                Ativo = servico.Ativo,
                Icone = servico.Icone,
                DataCriacao = servico.DataCriacao,
                CategoriaId = servico.CategoriaServicoId,
                CategoriaNome = servico.CategoriaServico?.Nome
            });
        }

        // POST /api/servicos
        /// <summary>Cria um novo serviço. Apenas administradores.</summary>
        [HttpPost]
        [Authorize(Policy = "Administrador")]
        public async Task<ActionResult<ServicoDto>> Criar([FromBody] CriarServicoDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Nome))
                return BadRequest(new { erro = "O nome do serviço é obrigatório." });

            if (dto.CategoriaId.HasValue)
            {
                var categoriaExiste = await _context.CategoriasServico.AnyAsync(c => c.Id == dto.CategoriaId);
                if (!categoriaExiste)
                    return BadRequest(new { erro = "Categoria não encontrada." });
            }

            var servico = new Servico
            {
                Nome = dto.Nome.Trim(),
                Descricao = dto.Descricao.Trim(),
                PrecoBase = dto.PrecoBase,
                Ativo = dto.Ativo,
                Icone = string.IsNullOrWhiteSpace(dto.Icone) ? "bi-globe" : dto.Icone.Trim(),
                CategoriaServicoId = dto.CategoriaId,
                DataCriacao = DateTime.UtcNow
            };

            _context.Servicos.Add(servico);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(Detalhe), new { id = servico.Id }, new ServicoDto
            {
                Id = servico.Id,
                Nome = servico.Nome,
                Descricao = servico.Descricao,
                PrecoBase = servico.PrecoBase,
                Ativo = servico.Ativo,
                Icone = servico.Icone,
                DataCriacao = servico.DataCriacao,
                CategoriaId = servico.CategoriaServicoId
            });
        }

        // PUT /api/servicos/{id}
        /// <summary>Atualiza um serviço existente. Apenas administradores.</summary>
        [HttpPut("{id}")]
        [Authorize(Policy = "Administrador")]
        public async Task<IActionResult> Atualizar(int id, [FromBody] CriarServicoDto dto)
        {
            var servico = await _context.Servicos.FindAsync(id);
            if (servico == null) return NotFound();

            if (dto.CategoriaId.HasValue)
            {
                var categoriaExiste = await _context.CategoriasServico.AnyAsync(c => c.Id == dto.CategoriaId);
                if (!categoriaExiste)
                    return BadRequest(new { erro = "Categoria não encontrada." });
            }

            servico.Nome = dto.Nome.Trim();
            servico.Descricao = dto.Descricao.Trim();
            servico.PrecoBase = dto.PrecoBase;
            servico.Ativo = dto.Ativo;
            servico.Icone = string.IsNullOrWhiteSpace(dto.Icone) ? "bi-globe" : dto.Icone.Trim();
            servico.CategoriaServicoId = dto.CategoriaId;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE /api/servicos/{id}
        /// <summary>Elimina um serviço. Apenas administradores. Falha se houver pedidos associados.</summary>
        [HttpDelete("{id}")]
        [Authorize(Policy = "Administrador")]
        public async Task<IActionResult> Eliminar(int id)
        {
            var servico = await _context.Servicos
                .Include(s => s.PedidoServicos)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (servico == null) return NotFound();

            if (servico.PedidoServicos.Any())
                return Conflict(new { erro = "Não é possível eliminar um serviço com pedidos associados. Desative-o em alternativa." });

            _context.Servicos.Remove(servico);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }

    // ── DTOs ──

    public class ServicoDto
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public decimal PrecoBase { get; set; }
        public bool Ativo { get; set; }
        public string Icone { get; set; } = string.Empty;
        public DateTime DataCriacao { get; set; }
        public int? CategoriaId { get; set; }
        public string? CategoriaNome { get; set; }
    }

    public class CriarServicoDto
    {
        public string Nome { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public decimal PrecoBase { get; set; }
        public bool Ativo { get; set; } = true;
        public string? Icone { get; set; }
        public int? CategoriaId { get; set; }
    }
}
