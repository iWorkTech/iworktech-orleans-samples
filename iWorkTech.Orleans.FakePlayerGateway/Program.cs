using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using iWorkTech.Orleans.Common;
using iWorkTech.Orleans.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Runtime;
using Orleans.Runtime.Configuration;

namespace iWorkTech.Orleans.FakePlayerGateway
{
    internal class Program
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

        private static Task DoClientWork(IGrainFactory client)
        {
            const int games = 10; // number of games to simulate
            const int playersPerGame = 4; // number of players in each game
            var sendInterval = TimeSpan.FromSeconds(2); // interval for sending updates
            const int iterations = 100;

            // Precreate base heartbeat data objects for each of the games.
            // We'll modify them before every time before sending.
            var heartbeats = new HeartbeatData[games];
            for (var i = 0; i < games; i++)
            {
                heartbeats[i] = new HeartbeatData {Game = Guid.NewGuid()};
                for (var j = 0; j < playersPerGame; j++)
                    heartbeats[i].Status.Players.Add(GetPlayerId(i * playersPerGame + j));
            }

            var iteration = 0;
            var presence = client.GetGrain<IPresenceGrain>(0); 
            // PresenceGrain is a StatelessWorker, so we use a single grain ID for auto-scale
            var promises = new List<Task>();

            while (iteration++ < iterations)
            {
                Console.WriteLine("Sending heartbeat series #{0}", iteration);

                promises.Clear();

                try
                {
                    for (var i = 0; i < games; i++)
                    {
                        heartbeats[i].Status.Score =
                            string.Format("{0}:{1}", iteration,
                                iteration > 5 ? iteration - 5 : 0); // Simultate a meaningful game score

                        // We serialize the HeartbeatData object to a byte[] 
                        // only to simulate the real life scenario where data comes in
                        // as a binary blob and requires an initial processing before it 
                        // can be routed to the proper destination.
                        // We could have sent the HeartbeatData object directly to the 
                        // game grain because we know the game ID.
                        // For the sake of simulation we just pretend we don't.
                        var t = presence.Heartbeat(HeartbeatDataDotNetSerializer.Serialize(heartbeats[i]));

                        promises.Add(t);
                    }

                    // Wait for all calls to finish.
                    // It is okay to block the thread here because it's a client program with no parallelism.
                    // One should never block a thread in grain code.
                    Task.WaitAll(promises.ToArray());
                }
                catch (Exception exc)
                {
                    Console.WriteLine("Exception: {0}", exc.GetBaseException());
                }

                Thread.Sleep(sendInterval);
            }

            return Task.CompletedTask;
        }

        /// <summary>
        ///     Generates GUIDs for player IDs
        /// </summary>
        private static Guid GetPlayerId(int playerIndex)
        {
            // For convenience, we generate a set of predefined subsequent GUIDs for players 
            // using this one as a base.
            var playerGuid = new Guid("{2349992C-860A-4EDA-9590-000000000000}").ToByteArray();
            playerGuid[15] = (byte) (playerGuid[15] + playerIndex);
            return new Guid(playerGuid);
        }
    }
}