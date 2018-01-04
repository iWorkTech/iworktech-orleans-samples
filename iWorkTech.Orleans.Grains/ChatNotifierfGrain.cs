using System;
using System.Threading;
using System.Threading.Tasks;
using iWorkTech.Orleans.Common;
using iWorkTech.Orleans.Interfaces;
using Orleans;
using Orleans.Concurrency;

namespace iWorkTech.Orleans.Grains
{
    [Reentrant]
    [StatelessWorker]
    public class ChatNotifierfGrain : Grain, IChatNotifierGrain
    {
        public Task NotifyMessage(ChatMessage message)
        {
            throw new NotImplementedException();
        }
    }
}