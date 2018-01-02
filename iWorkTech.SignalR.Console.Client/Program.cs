using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;

namespace iWorkTech.SignalR.Client
{
    internal class Program
    {
        private static HubConnection _connection;

        private static void Main(string[] args)
        {
            StartConnectionAsync();

            _connection.On<string, string>("broadcastMessage",
                (name, message) => {Console.WriteLine($"{name} said: {message}"); });

            Console.ReadLine();

            for (var i = 0; i < 5; i++)
            {
                _connection.InvokeAsync("send", "name", i, CancellationToken.None);
            }

            Console.ReadLine();

            DisposeAsync();
        }


        private static async Task StartConnectionAsync()
        {
            _connection = new HubConnectionBuilder()
                .WithUrl("http://localhost:60299/chat")
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