using System;
using System.Net.Http.Headers;
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
        public HubConnection Connection { get; private set; }

        public Task NotifyMessage(ChatMessage msg)
        {
            StartConnectionAsync();

            Console.WriteLine("Notify Signalr Server {0} {1} {2}", msg.ChatId, msg.Name,
                msg.Message);

            //Connection.InvokeAsync("send", message.Name, message.Message, CancellationToken.None);
            Connection.On<string, string>("broadcastMessage",
                (name, message) => { Console.WriteLine($"{msg.Name} said: {msg.Message}"); });

            return Task.CompletedTask;
        }


        public Task StartConnectionAsync()
        {
            Connection = new HubConnectionBuilder()
                .WithUrl("http://localhost:60299/chat")
                .WithConsoleLogger()
                .WithMessagePackProtocol()
                .WithTransport(TransportType.WebSockets)
                .Build();

            //Connection.InvokeAsync("send", message.Name, message.Message, CancellationToken.None);
            Connection.On<string, string>("broadcastMessage",
                (name, message) => { Console.WriteLine($"{name} said: {message}"); });

            return Connection.StartAsync();
        }

        public Task DisposeAsync()
        {
            return Connection.DisposeAsync();
        }
    }
}