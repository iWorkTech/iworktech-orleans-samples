using iWorkTech.Orleans.Common;
using Microsoft.AspNetCore.SignalR;

namespace iWorkTech.Orleans.Web.Core
{
    public class LocationHub : Hub
    {
        public void LocationUpdate(VelocityMessage message)
        {
            // Forward a single messages to all browsers
            Clients.All.InvokeAsync("LocationUpdate", message);
        }

        public void LocationUpdates(VelocityBatch messages)
        {
            // Forward a batch of messages to all browsers
            Clients.All.InvokeAsync("LocationUpdates", messages);
        }
    }
}
