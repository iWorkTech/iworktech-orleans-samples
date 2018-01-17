using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using iWorkTech.Orleans.Common;
using iWorkTech.Orleans.Interfaces;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Runtime;
using Orleans.Runtime.Configuration;

namespace iWorkTech.Orleans.FakeDeviceGateway
{
    internal class Program
    {
        // San Francisco: approximate boundaries.
        private const double SFLatMin = 37.708;
        private const double SFLatMax = 37.78;
        private const double SFLonMin = -122.50;
        private const double SFLonMax = -122.39;
        private static int _counter;
        private static readonly Random Rand = new Random();

        private static async Task SendMessage(IGrainFactory client, Model model)
        {
            // simulate the device moving
            model.Speed += Rand.NextDouble(-0.0001, 0.0001);
            model.Direction += Rand.NextDouble(-0.001, 0.001);

            var lastLat = model.Lat;
            var lastLon = model.Lon;

            UpdateDevicePosition(model);

            if (lastLat == model.Lat || lastLon == model.Lon)
            {
                // the device has hit the boundary, so reverse it's direction
                model.Speed = -model.Speed;
                UpdateDevicePosition(model);
            }

            // send the mesage to Orleans
            var device = client.GetGrain<IDeviceGrain>(model.DeviceId);
            Console.WriteLine("Lat: {0} :: Lon :{1}", model.Lat, model.Lon);
            await device.ProcessMessage(new DeviceMessage(model.Lat, model.Lon, _counter, model.DeviceId,
                DateTime.UtcNow));
            Interlocked.Increment(ref _counter);
        }

        private static void UpdateDevicePosition(Model model)
        {
            model.Lat += Math.Cos(model.Direction) * model.Speed;
            model.Lon += Math.Sin(model.Direction) * model.Speed;
            model.Lat = model.Lat.Cap(SFLatMin, SFLatMax);
            model.Lon = model.Lon.Cap(SFLonMin, SFLonMax);
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
                    var config = ClientConfiguration.LocalhostSilo();
                    client = new ClientBuilder()
                        .UseConfiguration(config)
                        .ConfigureApplicationParts(parts =>
                            parts.AddApplicationPart(typeof(IDeviceGrain).Assembly).WithReferences())
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

        private static void DoClientWork(IGrainFactory client)
        {
            // simulate 20 devices
            var devices = new List<Model>();
            for (var i = 0; i < 2; i++)
                devices.Add(new Model
                {
                    DeviceId = i,
                    Lat = Rand.NextDouble(SFLatMin, SFLatMax),
                    Lon = Rand.NextDouble(SFLonMin, SFLonMax),
                    Direction = Rand.NextDouble(-Math.PI, Math.PI),
                    Speed = Rand.NextDouble(0, 0.0005)
                });

            var timer = new System.Timers.Timer { Interval = 10000 };
            timer.Elapsed += (s, e) =>
            {
                Console.Write(". ");
                Interlocked.Exchange(ref _counter, 0);
            };
            timer.Start();

            // create a thread for each device, and continually move it's position
            foreach (var model in devices)
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
            public int DeviceId { get; set; }
            public double Lat { get; set; }
            public double Lon { get; set; }
            public double Direction { get; set; }
            public double Speed { get; set; }
        }
    }
}