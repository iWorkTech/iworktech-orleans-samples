using Microsoft.AspNetCore.SignalR;

namespace iWorkTech.Orleans.Web.Core.Hub
{
    public class ChatHub : Microsoft.AspNetCore.SignalR.Hub
    {
        public void Send(string name, string message)
        {
            // Call the broadcastMessage method to update clients.
            Clients.All.InvokeAsync("broadcastMessage", name, message);
        }

    }
}