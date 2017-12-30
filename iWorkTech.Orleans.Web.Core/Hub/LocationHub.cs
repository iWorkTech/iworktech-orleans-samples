using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using iWorkTech.Orleans.Common;
using Microsoft.AspNetCore.SignalR;

namespace iWorkTech.Orleans.Web.Core.Hub
{
    public class LocationHub : Microsoft.AspNetCore.SignalR.Hub
    {
        public async Task LocationUpdate(VelocityMessage message)
        {
            // Forward a single messages to all browsers
            await Clients.All.InvokeAsync("locationUpdate", message);
        }

        public async Task LocationUpdates(VelocityBatch messages)
        {
            // Forward a batch of messages to all browsers
            await Clients.All.InvokeAsync("locationUpdates", messages);
        }
    }
}
