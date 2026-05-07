using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebServicos.Data;
using WebServicos.Models;

namespace WebServicos.API.Controllers
{
    /// <summary>
    /// API REST para gestão de serviços.
    /// Endpoint base: /api/servicos
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class ServicosController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<ServicosController> _logger;

        public ServicosController(ApplicationDbContext db, ILogger<ServicosController> logger)
        {
            _db = db;
            _logger = logger;
        }

        // GET /api/servicos
        /// <summary>Obter todos os serviços ativos.</summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ServicoDto>>> GetServicos([FromQuery] bool? ativo)
        {
            var query = _db.Servicos.AsQueryable();

            if (ativo.HasValue)
                query = query.Where(s => s.Ativo == ativo.Value);

            var servicos = await query
                .OrderBy(s => s.Nome)
                .Select(s => new ServicoDto
                {
                    Id = s.Id,
                    Nome = s.Nome,
                    Descricao = s.Descricao,
                    PrecoBase = s.PrecoBase,
                    Ativo = s.Ativo,
                    Icone = s.Icone
                })
                .ToListAsync();

            return Ok(servicos);
        }

        // GET /api/servicos/{id}
        /// <summary>Obter detalhes de um serviço específico.</summary>
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ServicoDto>> GetServico(int id)
        {
            var servico = await _db.Servicos.FindAsync(id);
            if (servico == null)
                return NotFound(new { Mensagem = $"Serviço com ID {id} não encontrado." });

            return Ok(new ServicoDto
            {
                Id = servico.Id,
                Nome = servico.Nome,
                Descricao = servico.Descricao,
                PrecoBase = servico.PrecoBase,
                Ativo = servico.Ativo,
                Icone = servico.Icone
            });
        }

        // POST /api/servicos
        /// <summary>Criar um novo serviço.</summary>
        [HttpPost]
        public async Task<ActionResult<ServicoDto>> CreateServico([FromBody] CreateServicoDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var servico = new Servico
            {
                Nome = dto.Nome,
                Descricao = dto.Descricao,
                PrecoBase = dto.PrecoBase,
                Ativo = dto.Ativo,
                Icone = dto.Icone,
                DataCriacao = DateTime.UtcNow
            };

            _db.Servicos.Add(servico);
            await _db.SaveChangesAsync();

            _logger.LogInformation("API: Serviço '{Nome}' criado (ID: {Id}).", servico.Nome, servico.Id);

            return CreatedAtAction(
                nameof(GetServico),
                new { id = servico.Id },
                new ServicoDto { Id = servico.Id, Nome = servico.Nome, Descricao = servico.Descricao, PrecoBase = servico.PrecoBase, Ativo = servico.Ativo, Icone = servico.Icone });
        }

        // PUT /api/servicos/{id}
        /// <summary>Atualizar um serviço existente.</summary>
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateServico(int id, [FromBody] CreateServicoDto dto)
        {
            var servico = await _db.Servicos.FindAsync(id);
            if (servico == null)
                return NotFound(new { Mensagem = $"Serviço com ID {id} não encontrado." });

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            servico.Nome = dto.Nome;
            servico.Descricao = dto.Descricao;
            servico.PrecoBase = dto.PrecoBase;
            servico.Ativo = dto.Ativo;
            servico.Icone = dto.Icone;

            await _db.SaveChangesAsync();
            _logger.LogInformation("API: Serviço #{Id} atualizado.", id);

            return NoContent();
        }

        // DELETE /api/servicos/{id}
        /// <summary>Eliminar um serviço (apenas se sem pedidos associados).</summary>
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteServico(int id)
        {
            var servico = await _db.Servicos.FindAsync(id);
            if (servico == null)
                return NotFound(new { Mensagem = $"Serviço com ID {id} não encontrado." });

            var temPedidos = await _db.PedidoServicos.AnyAsync(ps => ps.ServicoId == id);
            if (temPedidos)
                return Conflict(new { Mensagem = "Não é possível eliminar um serviço com pedidos associados." });

            _db.Servicos.Remove(servico);
            await _db.SaveChangesAsync();

            _logger.LogInformation("API: Serviço #{Id} eliminado.", id);
            return NoContent();
        }

        // GET /api/servicos/estatisticas
        /// <summary>Obter estatísticas dos serviços.</summary>
        [HttpGet("estatisticas")]
        public async Task<ActionResult> GetEstatisticas()
        {
            var stats = await _db.Servicos
                .Select(s => new
                {
                    s.Id,
                    s.Nome,
                    TotalPedidos = s.PedidoServicos.Count,
                    PrecoMedio = s.PrecoBase
                })
                .OrderByDescending(x => x.TotalPedidos)
                .ToListAsync();

            return Ok(new
            {
                TotalServicos = await _db.Servicos.CountAsync(),
                TotalAtivos = await _db.Servicos.CountAsync(s => s.Ativo),
                PrecoMedio = await _db.Servicos.AverageAsync(s => (double)s.PrecoBase),
                Servicos = stats
            });
        }
    }

    // ── DTOs ──
    public class ServicoDto
    {
        public int Id { get; set; }
        public string Nome { get; set; } = "";
        public string Descricao { get; set; } = "";
        public decimal PrecoBase { get; set; }
        public bool Ativo { get; set; }
        public string Icone { get; set; } = "";
    }

    public class CreateServicoDto
    {
        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.StringLength(100)]
        public string Nome { get; set; } = "";

        [System.ComponentModel.DataAnnotations.Required]
        public string Descricao { get; set; } = "";

        [System.ComponentModel.DataAnnotations.Range(0, 99999.99)]
        public decimal PrecoBase { get; set; }

        public bool Ativo { get; set; } = true;

        public string Icone { get; set; } = "bi-globe";
    }
}
