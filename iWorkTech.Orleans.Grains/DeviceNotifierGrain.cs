using System;
using System.Collections.Generic;
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
    public class DeviceNotifierGrain : Grain, IDeviceNotifierGrain
    {
        private readonly List<VelocityMessage> _messageQueue = new List<VelocityMessage>();

        public async Task Notify(VelocityMessage message)
        {
            Console.WriteLine("Device ID:{0} Lat:{1} Lon: {2}", message.DeviceId, message.Latitude,
                message.Longitude);
            // add a message to the send queue
            _messageQueue.Add(message);
            if (_messageQueue.Count > 25)
                await Flush();
        }

        public override async Task OnActivateAsync()
        {
            // set up a timer to regularly flush the message queue
            RegisterTimer(FlushQueue, null, TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(100));

            await base.OnActivateAsync();
        }

        private async Task FlushQueue(object _)
        {
            await Flush();
        }

        private Task Flush()
        {
            if (_messageQueue.Count == 0) return Task.CompletedTask; 

            Console.WriteLine("Starting connection. Press Ctrl-C to close.");
            var cts = new CancellationTokenSource();
            Console.CancelKeyPress += (sender, a) =>
            {
                a.Cancel = true;
                cts.Cancel();
            };

            return Task.CompletedTask;
        }
    }
}