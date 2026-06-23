using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebServicos.Data;
using WebServicos.Models;
using System.Security.Claims;

namespace WebServicos.Controllers
{
    /// <summary>
    /// Controller para gerenciar as notificações dos utilizadores.
    /// Fornece endpoints para obter notificações recentes e marcar como lidas.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificacoesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public NotificacoesController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Obtém as notificações recentes do utilizador autenticado.
        /// Por defeito, retorna as últimas 20 notificações não lidas e lidas.
        /// </summary>
        /// <param name="limite">Número máximo de notificações a retornar (padrão: 20).</param>
        /// <returns>Lista de notificações ordenadas por data (mais recentes primeiro).</returns>
        [HttpGet("recentes")]
        public async Task<ActionResult<IEnumerable<NotificacaoDto>>> ObterNotificacoesRecentes([FromQuery] int limite = 20)
        {
            var utilizadorId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(utilizadorId))
                return Unauthorized();

            var notificacoes = await _context.Notificacoes
                .Where(n => n.UtilizadorId == utilizadorId)
                .OrderByDescending(n => n.DataHora)
                .Take(limite)
                .Select(n => new NotificacaoDto
                {
                    Id = n.Id,
                    Tipo = n.Tipo,
                    Titulo = n.Titulo,
                    Descricao = n.Descricao,
                    PedidoId = n.PedidoId,
                    DataHora = n.DataHora,
                    Lida = n.Lida
                })
                .ToListAsync();

            return Ok(notificacoes);
        }

        /// <summary>
        /// Conta o número de notificações não lidas do utilizador autenticado.
        /// </summary>
        /// <returns>Número de notificações não lidas.</returns>
        [HttpGet("nao-lidas")]
        public async Task<ActionResult<int>> ContagemNaoLidas()
        {
            var utilizadorId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(utilizadorId))
                return Unauthorized();

            var contagem = await _context.Notificacoes
                .CountAsync(n => n.UtilizadorId == utilizadorId && !n.Lida);

            return Ok(contagem);
        }

        /// <summary>
        /// Marca uma notificação específica como lida.
        /// </summary>
        /// <param name="id">ID da notificação a marcar como lida.</param>
        /// <returns>Status da operação.</returns>
        [HttpPost("{id}/marcar-lida")]
        public async Task<IActionResult> MarcarComoLida(int id)
        {
            var utilizadorId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(utilizadorId))
                return Unauthorized();

            var notificacao = await _context.Notificacoes.FindAsync(id);
            
            if (notificacao == null)
                return NotFound();

            if (notificacao.UtilizadorId != utilizadorId)
                return Forbid();

            notificacao.Lida = true;
            notificacao.DataLeitura = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok();
        }

        /// <summary>
        /// Marca todas as notificações do utilizador como lidas.
        /// </summary>
        /// <returns>Status da operação.</returns>
        [HttpPost("marcar-todas-lidas")]
        public async Task<IActionResult> MarcarTodasComoLidas()
        {
            var utilizadorId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(utilizadorId))
                return Unauthorized();

            var notificacoes = await _context.Notificacoes
                .Where(n => n.UtilizadorId == utilizadorId && !n.Lida)
                .ToListAsync();

            foreach (var notificacao in notificacoes)
            {
                notificacao.Lida = true;
                notificacao.DataLeitura = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            return Ok();
        }

        /// <summary>
        /// Elimina uma notificação específica.
        /// </summary>
        /// <param name="id">ID da notificação a eliminar.</param>
        /// <returns>Status da operação.</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarNotificacao(int id)
        {
            var utilizadorId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(utilizadorId))
                return Unauthorized();

            var notificacao = await _context.Notificacoes.FindAsync(id);
            
            if (notificacao == null)
                return NotFound();

            if (notificacao.UtilizadorId != utilizadorId)
                return Forbid();

            _context.Notificacoes.Remove(notificacao);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }

    /// <summary>
    /// DTO (Data Transfer Object) para retornar notificações na API.
    /// </summary>
    public class NotificacaoDto
    {
        public int Id { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public string Titulo { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public int? PedidoId { get; set; }
        public DateTime DataHora { get; set; }
        public bool Lida { get; set; }
    }
}
