﻿using Orleans.Runtime.Configuration;
using System;
using System.Threading.Tasks;
using iWorkTech.Orleans.Grains;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Hosting;
namespace iWorkTech.Orleans.SiloHost
{
    public class Program
    {
        public static int Main(string[] args)
        {
            return RunMainAsync().Result;
        }

        private static async Task<int> RunMainAsync()
        {
            try
            {
                var host = await StartSilo();
                Console.WriteLine("Press Enter to terminate...");
                Console.ReadLine();
                await host.StopAsync();

                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return 1;
            }
        }

        private static async Task<ISiloHost> StartSilo()
        {
            // define the cluster configuration
            var config = ClusterConfiguration.LocalhostPrimarySilo();
            config.AddMemoryStorageProvider();

            var builder = new SiloHostBuilder()
                .UseConfiguration(config)
                .ConfigureApplicationParts(parts => parts.AddFromAppDomain().AddFromApplicationBaseDirectory())
                .ConfigureLogging(logging => logging.AddConsole());

            var host = builder.Build();
            await host.StartAsync();
            return host;
        }
    }
}
