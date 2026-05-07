using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace WebServicos.Hubs
{
    /// <summary>
    /// Hub SignalR para notificações em tempo real.
    /// Utilizado para alertar administradores sobre novos pedidos.
    /// </summary>
    [Authorize]
    public class NotificacoesHub : Hub
    {
        /// <summary>
        /// Notifica todos os administradores conectados sobre um novo pedido.
        /// </summary>
        public async Task NotificarNovoPedido(string tituloProjeto, string nomeCliente)
        {
            await Clients.All.SendAsync("NovoPedido", tituloProjeto, nomeCliente, DateTime.Now.ToString("HH:mm"));
        }

        /// <summary>
        /// Notifica o cliente sobre uma atualização no estado do seu pedido.
        /// </summary>
        public async Task NotificarAtualizacaoPedido(string clienteId, string tituloProjeto, string novoEstado)
        {
            await Clients.User(clienteId).SendAsync("PedidoAtualizado", tituloProjeto, novoEstado, DateTime.Now.ToString("HH:mm"));
        }

        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await base.OnDisconnectedAsync(exception);
        }
    }
}
