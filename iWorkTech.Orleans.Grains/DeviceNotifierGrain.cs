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

            _connection = new HubConnectionBuilder()
                //.WithUrl("http://localhost:60299/location")
                //.WithConsoleLogger()
                .WithMessagePackProtocol()
                //.WithTransport(TransportType.WebSockets)
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

            Console.WriteLine("Starting connection. Press Ctrl-C to close.");
            var cts = new CancellationTokenSource();
            Console.CancelKeyPress += (sender, a) =>
            {
                a.Cancel = true;
                cts.Cancel();
            };

            _connection.Closed += e =>
            {
                Console.WriteLine("Connection closed with error: {0}", e);

                cts.Cancel();
            };

            _connection.On("marketOpened", async () => { await StartStreaming(); });

            // Do an initial check to see if we can start streaming the stocks
            var state = await _connection.InvokeAsync<string>("GetMarketState", cts.Token);
            if (string.Equals(state, "Open")) await StartStreaming();

            // Keep client running until cancel requested.
            while (!cts.IsCancellationRequested) await Task.Delay(250, cts.Token);

            async Task StartStreaming()
            {
                var channel = await _connection.StreamAsync<VelocityMessage>("StreamStocks", CancellationToken.None);
                while (await channel.WaitToReadAsync(cts.Token) && !cts.IsCancellationRequested)
                while (channel.TryRead(out var velocity))
                    Console.WriteLine($"{velocity.Latitude} {velocity.Longitude}");
            }

            try
            {
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}