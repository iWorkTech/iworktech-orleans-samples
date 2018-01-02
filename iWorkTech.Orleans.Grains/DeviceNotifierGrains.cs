using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using iWorkTech.Orleans.Common;
using iWorkTech.Orleans.Interfaces;
using Microsoft.AspNetCore.SignalR.Client;
using Orleans;
using Orleans.Concurrency;

namespace iWorkTech.Orleans.Grains
{
    [Reentrant]
    [StatelessWorker]
    public class DeviceNotifierGrain : Grain, IDeviceNotifierGrain
    {
        private readonly List<VelocityMessage> _messageQueue = new List<VelocityMessage>();
        private HubConnection _connection;

        public async Task SendMessage(VelocityMessage message)
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

            // not in azure, the SignalR hub is running locally
            //await AddHub("http://localhost:60299/location");
            //await AddHub("https://localhost:44358/location");

            _connection = new HubConnectionBuilder()
                .WithUrl("http://localhost:60299/location")
                .WithConsoleLogger()
                .Build();

            await _connection.StartAsync();
            await base.OnActivateAsync();
        }

        private async Task FlushQueue(object _)
        {
            await Flush();
        }

        private async Task Flush()
        {
            if (_messageQueue.Count == 0) return;

            // send all messages to all SignalR hubs
            var messagesToSend = _messageQueue.ToArray();
            _messageQueue.Clear();

            try
            {
               _connection.On<string, string>("locationUpdate",
                    (name, message) => { Console.WriteLine($"{name} said: {message}"); });

                foreach (var msg in messagesToSend)
                    await _connection.SendAsync("locationUpdate", msg, CancellationToken.None);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

    }
}