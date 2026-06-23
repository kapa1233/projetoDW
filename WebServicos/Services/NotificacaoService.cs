using WebServicos.Data;
using WebServicos.Models;
using Microsoft.AspNetCore.SignalR;
using WebServicos.Hubs;

namespace WebServicos.Services
{
    /// <summary>
    /// Serviço para criar e enviar notificações aos utilizadores.
    /// Integra-se com SignalR para notificações em tempo real e com a base de dados para histórico.
    /// </summary>
    public interface INotificacaoService
    {
        /// <summary>Cria uma notificação e envia em tempo real via SignalR.</summary>
        Task CriarNotificacaoAsync(string utilizadorId, string tipo, string titulo, string descricao, int? pedidoId = null);

        /// <summary>Cria uma notificação para o cliente quando um pedido é alterado.</summary>
        Task NotificarAtualizacaoPedidoAsync(int pedidoId, string titulo, string descricao);

        /// <summary>Cria uma notificação para o destinatário quando recebe uma nova mensagem.</summary>
        Task NotificarNovaMensagemAsync(int pedidoId, string remetenteNome, string conteudoMensagem, string destinatarioId);
    }

    /// <summary>
    /// Implementação do serviço de notificações.
    /// </summary>
    public class NotificacaoService : INotificacaoService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<NotificacoesHub> _hubContext;
        private readonly ILogger<NotificacaoService> _logger;

        public NotificacaoService(
            ApplicationDbContext context,
            IHubContext<NotificacoesHub> hubContext,
            ILogger<NotificacaoService> logger)
        {
            _context = context;
            _hubContext = hubContext;
            _logger = logger;
        }

        /// <summary>
        /// Cria uma notificação de forma assíncrona.
        /// </summary>
        public async Task CriarNotificacaoAsync(string utilizadorId, string tipo, string titulo, string descricao, int? pedidoId = null)
        {
            try
            {
                var notificacao = new Notificacao
                {
                    UtilizadorId = utilizadorId,
                    Tipo = tipo,
                    Titulo = titulo,
                    Descricao = descricao,
                    PedidoId = pedidoId,
                    DataHora = DateTime.UtcNow,
                    Lida = false
                };

                _context.Notificacoes.Add(notificacao);
                await _context.SaveChangesAsync();

                // Envia notificação em tempo real via SignalR (payload completo)
                await _hubContext.Clients.User(utilizadorId)
                    .SendAsync("NovaNotificacao", new
                    {
                        id = notificacao.Id,
                        tipo = notificacao.Tipo,
                        titulo = notificacao.Titulo,
                        descricao = notificacao.Descricao,
                        pedidoId = notificacao.PedidoId,
                        dataHora = notificacao.DataHora,
                        lida = notificacao.Lida
                    });

                _logger.LogInformation($"Notificação criada para utilizador {utilizadorId}: {tipo}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erro ao criar notificação: {ex.Message}");
            }
        }

        /// <summary>
        /// Notifica um cliente quando o seu pedido é atualizado.
        /// </summary>
        public async Task NotificarAtualizacaoPedidoAsync(int pedidoId, string titulo, string descricao)
        {
            try
            {
                var pedido = await _context.Pedidos.FindAsync(pedidoId);
                if (pedido == null)
                {
                    _logger.LogWarning($"Pedido {pedidoId} não encontrado para notificação");
                    return;
                }

                await CriarNotificacaoAsync(
                    pedido.ClienteId,
                    "PedidoAlterado",
                    titulo,
                    descricao,
                    pedidoId
                );
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erro ao notificar atualização de pedido: {ex.Message}");
            }
        }

        /// <summary>
        /// Notifica um cliente quando recebe uma nova mensagem de chat no seu pedido.
        /// </summary>
        public async Task NotificarNovaMensagemAsync(int pedidoId, string remetenteNome, string conteudoMensagem, string destinatarioId)
        {
            try
            {
                var pedido = await _context.Pedidos.FindAsync(pedidoId);
                if (pedido == null)
                {
                    _logger.LogWarning($"Pedido {pedidoId} não encontrado para notificação de mensagem");
                    return;
                }

                var descricao = conteudoMensagem.Length > 100
                    ? conteudoMensagem.Substring(0, 100) + "..."
                    : conteudoMensagem;

                await CriarNotificacaoAsync(
                    destinatarioId,
                    "NovaMensagem",
                    $"Nova mensagem de {remetenteNome}",
                    descricao,
                    pedidoId
                );
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erro ao notificar nova mensagem: {ex.Message}");
            }
        }
    }
}
