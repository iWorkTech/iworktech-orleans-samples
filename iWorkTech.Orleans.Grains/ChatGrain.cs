using System;
using System.Threading.Tasks;
using iWorkTech.Orleans.Common;
using iWorkTech.Orleans.Interfaces;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Concurrency;

namespace iWorkTech.Orleans.Grains
{
    /// <summary>
    ///     Orleans grain implementation class.
    /// </summary>
    [Reentrant]
    public class ChatGrain : Grain, IChatGrain
    {
        public Task ProcessMessage(ChatMessage message)
        {
            Console.WriteLine("ProcessMessage {0} {1} {2}", message.ChatId, message.Name, message.Message);
            var notifier = GrainFactory.GetGrain<IChatNotifierGrain>(0);
            notifier.NotifyMessage(message);
            return Task.CompletedTask;
        }
    }
}