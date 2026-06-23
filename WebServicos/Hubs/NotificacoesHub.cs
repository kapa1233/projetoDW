using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace WebServicos.Hubs
{
    /// <summary>
    /// Hub SignalR para notificações em tempo real entre o servidor e os clientes web.
    /// Permite alertar administradores sobre novos pedidos e notificar clientes
    /// sobre atualizações de estado, sem necessidade de recarregar a página.
    /// Requer autenticação: apenas utilizadores com sessão iniciada podem ligar.
    /// </summary>
    [Authorize]
    public class NotificacoesHub : Hub
    {
        /// <summary>
        /// Envia uma notificação a todos os clientes conectados quando um novo pedido é submetido.
        /// Chamado a partir do servidor (IHubContext) na página de criação de pedidos.
        /// </summary>
        /// <param name="tituloProjeto">Título do projeto do novo pedido.</param>
        /// <param name="nomeCliente">Nome do cliente que submeteu o pedido.</param>
        public async Task NotificarNovoPedido(string tituloProjeto, string nomeCliente)
        {
            // Envia para todos os utilizadores conectados (administradores e clientes)
            await Clients.All.SendAsync("NovoPedido", tituloProjeto, nomeCliente, DateTime.Now.ToString("HH:mm"));
        }

        /// <summary>
        /// Envia uma notificação ao cliente específico sobre uma atualização no estado do seu pedido.
        /// Chamado a partir do servidor quando o administrador altera o estado de um pedido.
        /// </summary>
        /// <param name="clienteId">ID do utilizador que deve receber a notificação.</param>
        /// <param name="tituloProjeto">Título do pedido que foi atualizado.</param>
        /// <param name="novoEstado">Novo estado do pedido (ex: "Em Desenvolvimento").</param>
        public async Task NotificarAtualizacaoPedido(string clienteId, string tituloProjeto, string novoEstado)
        {
            // Envia apenas para o utilizador com o ID especificado
            await Clients.User(clienteId).SendAsync("PedidoAtualizado", tituloProjeto, novoEstado, DateTime.Now.ToString("HH:mm"));
        }

        /// <summary>
        /// Chamado automaticamente pelo SignalR quando um utilizador se conecta ao hub.
        /// </summary>
        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
        }

        /// <summary>
        /// Chamado automaticamente pelo SignalR quando um utilizador se desconecta do hub.
        /// </summary>
        /// <param name="exception">Exceção que causou a desconexão (null se foi intencional).</param>
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await base.OnDisconnectedAsync(exception);
        }
    }
}
