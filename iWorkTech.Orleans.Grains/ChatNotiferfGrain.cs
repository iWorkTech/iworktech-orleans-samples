using System;
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
    public class ChatNotiferfGrain : Grain, IChatNotifierGrain
    {
        private HubConnection _connection;

        public Task NotifyMessage(ChatMessage message)
        {
            Console.WriteLine("NotifyMessage Chat ID:{0} Name:{1} Message: {2}", message.ChatId, message.Name,
                message.Message);

            Console.WriteLine("Sending Messages...");
            _connection.InvokeAsync("send", message.Name, message.Message, CancellationToken.None);

            return Task.CompletedTask;
        }

        public override async Task OnActivateAsync()
        {
            await StartConnectionAsync();

            _connection.On<string, string>("broadcastMessage",
                (name, message) => { Console.WriteLine($"{name} said: {message}"); });

            await base.OnActivateAsync();
        }

        public async Task StartConnectionAsync()
        {
            _connection = new HubConnectionBuilder()
                .WithUrl("http://localhost:60299/chat")
                .WithConsoleLogger()
                .Build();

            await _connection.StartAsync();
        }

        public async Task DisposeAsync()
        {
            await _connection.DisposeAsync();
        }
    }
}