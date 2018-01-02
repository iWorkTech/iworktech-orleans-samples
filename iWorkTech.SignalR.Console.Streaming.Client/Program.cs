using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;

namespace iWorkTech.SignalR.Console.Streaming.Client
{
    class Program
    {
        private static HubConnection _connection;
        static void Main(string[] args)
        {

            StartConnectionAsync();
            _connection.On("streamStarted", () =>
            {
                StartStreaming();
            });
            //Console.ReadLine();
            DisposeAsync();
        }


        public static async Task StartStreaming()
        {
            var channel = await _connection.StreamAsync<string>("StartStreaming", CancellationToken.None);
            while (await channel.WaitToReadAsync())
            {
                while (channel.TryRead(out string message))
                {
                    //Console.WriteLine($"Message received: {message}");
                }
            }
        }

        public static async Task StartConnectionAsync()
        {
            _connection = new HubConnectionBuilder()
                .WithUrl("http://localhost:60299/streaming")
                .WithConsoleLogger()
                .Build();

            await _connection.StartAsync();
        }

        public static async Task DisposeAsync()
        {
            await _connection.DisposeAsync();
        }
    }
}
