using System;
using System.Threading.Tasks;
using iWorkTech.Orleans.Grains;
using iWorkTech.Orleans.Interfaces;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Hosting;
using Orleans.Runtime.Configuration;

namespace iWorkTech.Orleans.SiloHost
{
    internal class Program
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
                if (host.Stopped.IsCompleted)
                    host.Dispose();

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

            var siloConfig = ClusterConfiguration.LocalhostPrimarySilo();
            siloConfig.AddMemoryStorageProvider();

            var silo = new SiloHostBuilder()
                .ConfigureApplicationParts(parts =>
                    parts.AddApplicationPart(typeof(IPlayerGrain).Assembly).WithReferences())
                .ConfigureApplicationParts(parts =>
                    parts.AddApplicationPart(typeof(PlayerGrain).Assembly).WithReferences())
                .ConfigureLogging(logging => logging.AddConsole());

            //silo.UseSignalR();

            var host = silo.Build();
            await host.StartAsync();
            return host;
        }
    }
}