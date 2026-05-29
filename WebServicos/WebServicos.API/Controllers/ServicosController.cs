using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebServicos.Data;
using WebServicos.Models;

namespace WebServicos.API.Controllers
{
    /// <summary>
    /// API REST para gestão de serviços da plataforma WebServicos.
    /// Disponibiliza endpoints CRUD completos e estatísticas de utilização.
    /// Endpoint base: /api/servicos
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class ServicosController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<ServicosController> _logger;

        /// <summary>
        /// Construtor com injeção de dependências (contexto da BD e logger).
        /// </summary>
        public ServicosController(ApplicationDbContext db, ILogger<ServicosController> logger)
        {
            _db = db;
            _logger = logger;
        }

        /// <summary>
        /// Obtém a lista de serviços, com filtro opcional por estado ativo/inativo.
        /// GET /api/servicos?ativo=true
        /// </summary>
        /// <param name="ativo">Filtrar por estado ativo (true/false). Se omitido, retorna todos.</param>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ServicoDto>>> GetServicos([FromQuery] bool? ativo)
        {
            var query = _db.Servicos.AsQueryable();

            // Filtrar por estado ativo se o parâmetro foi fornecido
            if (ativo.HasValue)
                query = query.Where(s => s.Ativo == ativo.Value);

            // Projeção para DTO evita expor campos desnecessários
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

        /// <summary>
        /// Obtém os detalhes de um serviço específico pelo seu ID.
        /// GET /api/servicos/{id}
        /// </summary>
        /// <param name="id">ID do serviço a consultar.</param>
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

        /// <summary>
        /// Cria um novo serviço na plataforma.
        /// POST /api/servicos
        /// </summary>
        /// <param name="dto">Dados do serviço a criar.</param>
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

            // Retorna 201 Created com a localização do novo recurso
            return CreatedAtAction(
                nameof(GetServico),
                new { id = servico.Id },
                new ServicoDto { Id = servico.Id, Nome = servico.Nome, Descricao = servico.Descricao, PrecoBase = servico.PrecoBase, Ativo = servico.Ativo, Icone = servico.Icone });
        }

        /// <summary>
        /// Atualiza todos os campos de um serviço existente.
        /// PUT /api/servicos/{id}
        /// </summary>
        /// <param name="id">ID do serviço a atualizar.</param>
        /// <param name="dto">Novos dados do serviço.</param>
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateServico(int id, [FromBody] CreateServicoDto dto)
        {
            var servico = await _db.Servicos.FindAsync(id);
            if (servico == null)
                return NotFound(new { Mensagem = $"Serviço com ID {id} não encontrado." });

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Atualizar todos os campos editáveis
            servico.Nome = dto.Nome;
            servico.Descricao = dto.Descricao;
            servico.PrecoBase = dto.PrecoBase;
            servico.Ativo = dto.Ativo;
            servico.Icone = dto.Icone;

            await _db.SaveChangesAsync();
            _logger.LogInformation("API: Serviço #{Id} atualizado.", id);

            return NoContent(); // 204 No Content (sucesso sem corpo de resposta)
        }

        /// <summary>
        /// Elimina um serviço, desde que não tenha pedidos associados.
        /// DELETE /api/servicos/{id}
        /// </summary>
        /// <param name="id">ID do serviço a eliminar.</param>
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteServico(int id)
        {
            var servico = await _db.Servicos.FindAsync(id);
            if (servico == null)
                return NotFound(new { Mensagem = $"Serviço com ID {id} não encontrado." });

            // Impede eliminação se existirem pedidos associados (integridade referencial)
            var temPedidos = await _db.PedidoServicos.AnyAsync(ps => ps.ServicoId == id);
            if (temPedidos)
                return Conflict(new { Mensagem = "Não é possível eliminar um serviço com pedidos associados." });

            _db.Servicos.Remove(servico);
            await _db.SaveChangesAsync();

            _logger.LogInformation("API: Serviço #{Id} eliminado.", id);
            return NoContent();
        }

        /// <summary>
        /// Obtém estatísticas agregadas de utilização dos serviços.
        /// Inclui total de pedidos por serviço, ordenado pelo mais requisitado.
        /// GET /api/servicos/estatisticas
        /// </summary>
        [HttpGet("estatisticas")]
        public async Task<ActionResult> GetEstatisticas()
        {
            // Agrega dados de utilização por serviço
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

    /// <summary>
    /// DTO de leitura para expor dados de um serviço via API.
    /// </summary>
    public class ServicoDto
    {
        /// <summary>ID do serviço.</summary>
        public int Id { get; set; }

        /// <summary>Nome do serviço.</summary>
        public string Nome { get; set; } = "";

        /// <summary>Descrição do serviço.</summary>
        public string Descricao { get; set; } = "";

        /// <summary>Preço base em euros.</summary>
        public decimal PrecoBase { get; set; }

        /// <summary>Indica se o serviço está ativo.</summary>
        public bool Ativo { get; set; }

        /// <summary>Classe do ícone Bootstrap Icons.</summary>
        public string Icone { get; set; } = "";
    }

    /// <summary>
    /// DTO de escrita para criar ou atualizar um serviço via API.
    /// </summary>
    public class CreateServicoDto
    {
        /// <summary>Nome do serviço (obrigatório, máx. 100 caracteres).</summary>
        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.StringLength(100)]
        public string Nome { get; set; } = "";

        /// <summary>Descrição detalhada do serviço (obrigatória).</summary>
        [System.ComponentModel.DataAnnotations.Required]
        public string Descricao { get; set; } = "";

        /// <summary>Preço base do serviço em euros (entre 0 e 99999,99).</summary>
        [System.ComponentModel.DataAnnotations.Range(0, 99999.99)]
        public decimal PrecoBase { get; set; }

        /// <summary>Indica se o serviço está disponível para novos pedidos.</summary>
        public bool Ativo { get; set; } = true;

        /// <summary>Classe do ícone Bootstrap Icons (ex: "bi-globe").</summary>
        public string Icone { get; set; } = "bi-globe";
    }
}
