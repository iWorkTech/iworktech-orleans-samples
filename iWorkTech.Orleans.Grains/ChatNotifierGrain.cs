using System;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using iWorkTech.Orleans.Common;
using iWorkTech.Orleans.Interfaces;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.SignalR.Internal.Protocol;
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

        public async Task NotifyMessage(ChatMessage msg)
        {
            try
            {
                await StartConnectionAsync();

                Connection.On<string, string>("broadcastMessage",
                    (name, message) => { Console.WriteLine($"{name} said: {message}"); });

                Console.WriteLine("Starting client work");
                await DoClientWork(msg);
                Console.WriteLine("Finished client work");
                await DisposeAsync();
                Console.WriteLine("Disposing hub connection");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private async Task StartConnectionAsync()
        {
            Connection = new HubConnectionBuilder()
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

            Connection.Closed += e =>
            {
                Console.WriteLine("Connection closed with error: {0}", e);
                cts.Cancel();
            };

            Connection.On<string, string>("broadcastMessage",
                (name, message) => { Console.WriteLine($"{name} said: {message}"); });

            await Connection.StartAsync();
            Console.WriteLine("Client successfully connected to hub");
        }

        public async Task DisposeAsync()
        {
            await Connection.DisposeAsync();
        }

        private async Task DoClientWork(ChatMessage msg)
        {
            Console.WriteLine("Sending messages to Hub");
            await Connection.InvokeAsync("send", msg.Name, msg.Message, CancellationToken.None);
            Console.WriteLine("Finished sending messages to Hub");

        }
    }
}

//public class ChatNotifierGrain : Grain, IChatNotifierGrain
//{
//    public HubConnection Connection { get; private set; }

//    public async Task NotifyMessage(ChatMessage msg)
//    {
//        await StartConnectionAsync();

//        Console.WriteLine("Notify Signalr Server {0} {1} {2}", msg.ChatId, msg.Name,
//            msg.Message);

//        await Connection.InvokeAsync("send", msg.Name, msg.Message, CancellationToken.None);
//    }

//    public async Task StartConnectionAsync()
//    {
//        Connection = new HubConnectionBuilder()
//            .WithUrl("http://localhost:60299/chat")
//            .WithConsoleLogger()
//            .WithMessagePackProtocol()
//            .WithTransport(TransportType.WebSockets)
//            .Build();

//        Console.WriteLine("Starting connection. Press Ctrl-C to close.");

//        var cts = new CancellationTokenSource();
//        Console.CancelKeyPress += (sender, a) =>
//        {
//            a.Cancel = true;
//            cts.Cancel();
//        };

//        Connection.Closed += e =>
//        {
//            Console.WriteLine("Connection closed with error: {0}", e);
//            cts.Cancel();
//        };

//        Connection.On<string, string>("broadcastMessage",
//            (name, message) => { Console.WriteLine($"{name} said: {message}"); });

//        await Connection.StartAsync();
//    }

//    public async Task DisposeAsync()
//    {
//        await Connection.DisposeAsync();
//    }
//}
