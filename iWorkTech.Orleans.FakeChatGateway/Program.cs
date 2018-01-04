using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
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

        private static async Task SendMessage(IGrainFactory client, Model model)
        {
            model.Name = "Gateway";

            // send the mesage to Orleans
            var chat = client.GetGrain<IChatGrain>(model.ChatId);
            Console.WriteLine("NotifyMessage ChatId: {0} :: Name: {1} :: Message :{2}", model.ChatId, model.Name,
                model.Message);
            await chat.ProcessMessage(new ChatMessage(model.ChatId, model.Name, model.Message));
            Interlocked.Increment(ref _counter);
        }

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
                    DoClientWork(client);
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
                    var config = ClientConfiguration.LocalhostSilo()
                        .AddSignalR(); 

                    client = new ClientBuilder()
                        .UseConfiguration(config)
                        .ConfigureApplicationParts(parts =>
                            parts.AddApplicationPart(typeof(IChatGrain).Assembly).WithReferences())
                        .ConfigureLogging(logging => logging.AddConsole())
                        .UseSignalR()
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

        private static void DoClientWork(IGrainFactory client)
        {
            // simulate 5 people chating
            var chats = new List<Model>();
            for (_counter = 0; _counter < 5; _counter++)
                chats.Add(new Model
                {
                    ChatId = _counter,
                    Name = "System",
                    Message = "Test Msg " + Rand.Next()
                });

            var timer = new System.Timers.Timer { Interval = 1000};
            timer.Elapsed += (s, e) =>
            {
                Console.Write(". ");
                Interlocked.Exchange(ref _counter, 0);
            };
            timer.Start();

            // create a thread for each device, and continually move it's position
            foreach (var model in chats)
            {
                var ts = new ThreadStart(async () =>
                {
                    while (true)
                        try
                        {
                            await SendMessage(client, model);
                            Thread.Sleep(Rand.Next(500, 2500));
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.ToString());
                        }
                });
                new Thread(ts).Start();
            }
        }

        private class Model
        {
            public string Name { get; set; }
            public string Message { get; set; }
            public int ChatId { get; set; }
        }
    }
}