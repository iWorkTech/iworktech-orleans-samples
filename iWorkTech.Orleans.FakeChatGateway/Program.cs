using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using iWorkTech.Orleans.Common;
using iWorkTech.Orleans.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Runtime;
using Orleans.Runtime.Configuration;

namespace iWorkTech.Orleans.FakeChatGateway
{
    internal class Program
    {
        private static int _counter;
        private static readonly Random Rand = new Random();

        private static int Main(string[] args)
        {
            return RunMainAsync().Result;
        }

        private static async Task<int> RunMainAsync()
        {
            try
            {
                using (var client = await StartClientWithRetries())
                {
                    await DoClientWork(client);
                    Console.ReadKey();
                }

                return 0;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return 1;
            }
        }

        private static async Task<IClusterClient> StartClientWithRetries(int initializeAttemptsBeforeFailing = 5)
        {
            var attempt = 0;
            IClusterClient client;
            while (true)
                try
                {
                    var config = ClientConfiguration.LocalhostSilo();
                        //.AddSignalR(); 

                    client = new ClientBuilder()
                        .UseConfiguration(config)
                        .ConfigureApplicationParts(parts =>
                            parts.AddApplicationPart(typeof(IChatGrain).Assembly).WithReferences())
                        .ConfigureLogging(logging => logging.AddConsole())
                        .Build();

                    await client.Connect();
                    Console.WriteLine("Client successfully connect to silo host");
                    break;
                }
                catch (SiloUnavailableException)
                {
                    attempt++;
                    Console.WriteLine(
                        $"Attempt {attempt} of {initializeAttemptsBeforeFailing} failed to initialize the Orleans client.");
                    if (attempt > initializeAttemptsBeforeFailing)
                        throw;
                    await Task.Delay(TimeSpan.FromSeconds(4));
                }

            return client;
        }

        private static Task DoClientWork(IClusterClient client)
        {
            var chatGrain = client.GetGrain<IChatGrain>(0);
            var chats = new List<ChatMessage>();
            for (_counter = 0; _counter < 5; _counter++)
                chats.Add(new ChatMessage
                {
                    ChatId = _counter,
                    Name = "System",
                    Message = "Test Msg " + Rand.Next()
                });

            foreach (var msg in chats)
            {
                Console.WriteLine("Send Message to Orleans Silo {0} {1} {2}", msg.ChatId, msg.Name,
                    msg.Message);

                chatGrain.ProcessMessage(msg);
            }

            return Task.CompletedTask;
        }
    }
}