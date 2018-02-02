using System;
using System.Threading.Tasks;
using iWorkTech.Orleans.Common;
using iWorkTech.Orleans.Interfaces;
using Orleans;
using Orleans.Concurrency;

namespace iWorkTech.Orleans.Grains
{
    [Reentrant]
    [StatelessWorker]
    public class ChatNotifierGrain : Grain, IChatNotifierGrain
    {
        public Task NotifyMessage(ChatMessage msg)
        {
            Console.WriteLine("Notify Signalr Server {0} {1} {2}", msg.ChatId, msg.Name,
                msg.Message);
            //DisposeAsync();

            return Task.CompletedTask;
        }
    }
}