using Microsoft.AspNetCore.SignalR;

namespace CashierQueueAPI.Hubs
{
    public class GlobalHub : Hub
    {
        public async Task JoinSection(string section)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, section);
        }

        public async Task EnviarAsignacion(int nCaja, string seccion)
        {
            await Clients.Group(seccion).SendAsync("AsignacionRecibida", new { nCaja, seccion });
        }
    }
}
