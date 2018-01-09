using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using iWorkTech.Orleans.Common;
using iWorkTech.Orleans.Interfaces;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.Sockets;
using Orleans;
using Orleans.Concurrency;

namespace iWorkTech.Orleans.Grains
{
    [Reentrant]
    [StatelessWorker]
    public class ChatNotifierfGrain : Grain, IChatNotifierGrain
    {
        private readonly List<ChatMessage> _messageQueue = new List<ChatMessage>();
        private HubConnection _connection;

        public async Task NotifyMessage(ChatMessage message)
        {
            Console.WriteLine("Device ID:{0} Lat:{1} Lon: {2}", message.Name, message.Message);
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
                .WithUrl("http://localhost:60299/streaming")
                .WithConsoleLogger()
                .WithMessagePackProtocol()
                .WithTransport(TransportType.WebSockets)
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
                return Task.CompletedTask;
            };

            _connection.On("marketOpened", async () =>
            {
                await Notify();
            });

            // Do an initial check to see if we can start streaming the chat
            var state = await _connection.InvokeAsync<string>("GetMarketState", cancellationToken: cts.Token);
            if (string.Equals(state, "Open"))
            {
                await Notify();
            }

            // Keep client running until cancel requested.
            while (!cts.IsCancellationRequested)
            {
                await Task.Delay(250, cts.Token);
            }

            async Task Notify()
            {
                var channel = await _connection.StreamAsync<ChatMessage>("StreamStocks", CancellationToken.None);
                while (await channel.WaitToReadAsync(cts.Token) && !cts.IsCancellationRequested)
                {
                    while (channel.TryRead(out var chat))
                    {
                        Console.WriteLine($"{chat.Name} {chat.Message}");
                    }
                }
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
