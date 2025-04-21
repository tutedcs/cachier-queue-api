using Microsoft.AspNetCore.SignalR;

namespace CashierQueueAPI.Hubs
{
    public class GlobalHub : Hub
    {
        // Unirse a una sección (grupo)
        public async Task JoinSection(string section)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, section);
        }

        // Enviar la asignación solo a esa sección
        public async Task EnviarAsignacion(int nCaja, string seccion)
        {
            await Clients.Group(seccion).SendAsync("AsignacionRecibida", new { nCaja, seccion });
        }
    }
}
