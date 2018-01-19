using System;
using System.Threading.Tasks;
using iWorkTech.Orleans.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Runtime;
using Orleans.Runtime.Configuration;

namespace iWorkTech.Orleans.PlayerWatcher
{
    internal partial class Program
    {
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
                    var config = ClientConfiguration.LocalhostSilo()
                        .AddSignalR();

                    client = new ClientBuilder()
                        .UseConfiguration(config)
                        //.UseSignalR()
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

        private static async Task DoClientWork(IGrainFactory client)
        {
            try
            {
                // Hardcoded player ID
                var playerId = new Guid("{2349992C-860A-4EDA-9590-000000000006}");
                var player = client.GetGrain<IPlayerGrain>(playerId);
                IGameGrain game = null;

                while (game == null)
                {
                    Console.WriteLine("Getting current game for player {0}...", playerId);

                    try
                    {
                        game = await player.GetCurrentGame();
                        if (game == null) // Wait until the player joins a game
                            await Task.Delay(5000);
                    }
                    catch (Exception exc)
                    {
                        Console.WriteLine("Exception: {0}", exc.GetBaseException().Message);
                    }
                }

                Console.WriteLine("Subscribing to updates for game {0}...", game.GetPrimaryKey());

                // Subscribe for updates
                var watcher = new GameObserver();
                await game.SubscribeForGameUpdates(
                    await client.CreateObjectReference<IGameObserver>(watcher));

                Console.WriteLine("Subscribed successfully. Press <Enter> to stop.");
            }
            catch (Exception exc)
            {
                Console.WriteLine("Unexpected Error: {0}", exc.GetBaseException().Message);
                throw;
            }
        }
    }
}