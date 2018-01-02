using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
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
        private readonly List<ChatMessage> _messageQueue = new List<ChatMessage>();
        private HubConnection _connection;

        public async Task NotifyMessage(ChatMessage message)
        {
            Console.WriteLine("NotifyMessage Chat ID:{0} Name:{1} Message: {2}", message.ChatId, message.Name,
                message.Message);
            // add a message to the send queue
            _messageQueue.Add(message);
            if (_messageQueue.Count > 5)
                await Flush();
        }

        public override async Task OnActivateAsync()
        {
            // set up a timer to regularly flush the message queue
            RegisterTimer(FlushQueue, null, TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(100));

            await StartConnectionAsync();
            _connection.On<string, string>("broadcastMessage",
                (name, message) => { Console.WriteLine($"{name} said: {message}"); });

            await base.OnActivateAsync();
        }

        private async Task FlushQueue(object _)
        {
            await Flush();
        }

        public async Task Flush()
        {
            if (_messageQueue.Count == 0) return;

            try
            {
                Console.WriteLine("Flusing Messages...");

                // send all messages to all SignalR hubs
                var messagesToSend = _messageQueue.ToArray();
                _messageQueue.Clear();

                foreach (var msg in messagesToSend)
                {
                    _connection.On<string, string>("broadcastMessage", (name, message) =>
                    {
                        Console.WriteLine($"{msg.Name} said: {msg.Message}");

                    });
                }

                await DisposeAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
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