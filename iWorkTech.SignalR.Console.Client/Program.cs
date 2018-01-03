using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;

namespace iWorkTech.SignalR.Client
{
    internal class Program
    {
        public static HubConnection Connection { get; set; }

        private static int Main(string[] args)
        {
            return RunMainAsync().Result;
        }

        private static async Task<int> RunMainAsync()
        {
            try
            {
                await StartConnectionAsync();

                Connection.On<string, string>("broadcastMessage",
                    (name, message) => { Console.WriteLine($"{name} said: {message}"); });

                Console.WriteLine("Starting client work");
                await DoClientWork();
                Console.WriteLine("Finished client work");
                await DisposeAsync();
                Console.WriteLine("Disposing hub connection");
                return 0;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return 1;
            }
        }

        private static async Task StartConnectionAsync()
        {
            Connection = new HubConnectionBuilder()
                .WithUrl("http://localhost:60299/chat")
                .WithConsoleLogger()
                .Build();

            await Connection.StartAsync();
            Console.WriteLine("Client successfully connected to hub");
        }

        public static async Task DisposeAsync()
        {
            await Connection.DisposeAsync();
        }

        private static async Task DoClientWork()
        {
            Console.WriteLine("Sending messages to Hub");
            for (var i = 0; i < 5; i++) await Connection.InvokeAsync("send", "name", i, CancellationToken.None);
            Console.WriteLine("Finished sending messages to Hub");

        }
    }
}