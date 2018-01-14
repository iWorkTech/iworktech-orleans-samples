using System;
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
    public class ChatNotifierGrain : Grain, IChatNotifierGrain
    {
        private HubConnection _connection;

        public Task NotifyMessage(ChatMessage msg)
        {
            StartConnectionAsync();

            Console.WriteLine("Notify Signalr Server {0} {1} {2}", msg.ChatId, msg.Name,
                msg.Message);


            _connection.InvokeAsync("send", msg.Name, msg.Message, CancellationToken.None);

            //DisposeAsync();

            return Task.CompletedTask;
        }

        public Task StartConnectionAsync()
        {
            if (_connection != null) return Task.CompletedTask;
            _connection = new HubConnectionBuilder()
                .WithUrl("http://localhost:60299/chat")
                .WithConsoleLogger()
                .WithMessagePackProtocol()
                .WithTransport(TransportType.WebSockets)
                .Build();

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

            _connection.On<string, string>("broadcastMessage",
                (name, message) => { Console.WriteLine($"{name} said: {message}"); });

            return _connection.StartAsync();
        }

        public Task DisposeAsync()
        {
            return _connection.DisposeAsync();
        }
    }
}