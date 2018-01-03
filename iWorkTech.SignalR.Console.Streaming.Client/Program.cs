using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;

namespace iWorkTech.SignalR.Streaming.Client
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
                Console.WriteLine("Starting client work and wating for messages from Hub");
                Connection.On("streamStarted", async () => { await DoClientWork(); });
                Console.ReadLine();
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
                .WithUrl("http://localhost:60299/streaming")
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
            Console.WriteLine("Recieving messages from Hub");
            var channel = await Connection.StreamAsync<string>("StartStreaming", CancellationToken.None);
            while (await channel.WaitToReadAsync())
            {
                while (channel.TryRead(out var message))
                {
                    Console.WriteLine($"Message received: {message}");
                }
            }

            Console.WriteLine("Wating for messages from Hub");
            Console.WriteLine("Press Enter to terminate");

        }
    }
}