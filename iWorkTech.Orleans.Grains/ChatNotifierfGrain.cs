using System;
using System.Threading;
using System.Threading.Tasks;
using iWorkTech.Orleans.Common;
using iWorkTech.Orleans.Interfaces;
using Orleans;
using Orleans.Concurrency;
using SignalR.Orleans.Core;

namespace iWorkTech.Orleans.Grains
{
    [Reentrant]
    [StatelessWorker]
    public class ChatNotifierfGrain : Grain, IChatNotifierGrain
    {
         private HubContext<IChatNotifierGrain> _hubContext;

        public Task NotifyMessage(ChatMessage message)
        {
            throw new NotImplementedException();
        }
    }
}