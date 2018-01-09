using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace iWorkTech.Orleans.Web.Core.Hub
{
    public class DrawHub : Microsoft.AspNetCore.SignalR.Hub
    {
        public override Task OnConnectedAsync()
        {
            Clients.All.InvokeAsync("draw", "system", $"{Context.ConnectionId} joined");
            return base.OnConnectedAsync();
        }

        public Task Draw(int prevX, int prevY, int currentX, int currentY, string color)
        {
            return Clients.AllExcept(new List<string> {Context.ConnectionId})
                .InvokeAsync("draw", prevX, prevY, currentX, currentY, color);
        }

        public override Task OnDisconnectedAsync(System.Exception exception)
        {
            Clients.All.InvokeAsync("draw", "system", $"{Context.ConnectionId} left");
            return base.OnDisconnectedAsync(exception);
        }
    }
}