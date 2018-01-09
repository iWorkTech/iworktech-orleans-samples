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
        public async Task ProcessMessage(ChatMessage message)
        {
            var notifier = GrainFactory.GetGrain<IChatNotifierGrain>(0);

            await notifier.NotifyMessage(message);

        }
    }
}