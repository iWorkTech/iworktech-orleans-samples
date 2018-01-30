using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using iWorkTech.Orleans.Grains;
using iWorkTech.Orleans.Grains.Redux;
using iWorkTech.Orleans.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
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
                {
                    host.Dispose();
                }

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
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddInMemoryCollection(
                    new Dictionary<string, string> // add default settings, that will be overridden by commandline
                    {
                        {"Id", "OrleansHost"},
                        {"Version", "1.0.0"},
                        {"DeploymentId", "testdeploymentid"}
                    })
                .AddCommandLine(args)
                .AddJsonFile("appconfig.json", true)
                .AddJsonFile($"appconfig.{environment}.json", true)
                .AddEnvironmentVariables(
                    "ASPNETCORE_"); // The CloudService will pass settings (such as) the connectionstring through environment variables

            if ("Development".Equals(environment) && builder.GetFileProvider().GetFileInfo("OrleansHost.csproj").Exists)
                builder.AddUserSecrets<Program>();


            var config = builder.Build();

            LoggerFactory.AddConsole(config.GetSection("Logging"));
            LoggerFactory.AddDebug();
            var logger = LoggerFactory.CreateLogger<Program>();

            logger.LogWarning("Starting Orleans silo...");
            
            var clusterConfig = ClusterConfiguration.LocalhostPrimarySilo();
            clusterConfig.Globals.ClusterId = config["Id"];
            clusterConfig.Globals.DataConnectionString = config.GetConnectionString("DataConnectionString");
            clusterConfig.AddMemoryStorageProvider("Default");
            clusterConfig.AddMemoryStorageProvider("PubSubStore");
            clusterConfig.AddSimpleMessageStreamProvider("Default");
            clusterConfig.AddSignalR();
            clusterConfig.AddMemoryStorageProvider();
            var siloName = config["Id"];

            var host = new SiloHostBuilder()
                .UseConfiguration(clusterConfig)
                .ConfigureSiloName(siloName)
                .ConfigureServices(services =>
                {
                    services.AddOptions();
                    services.TryAdd(ServiceDescriptor.Singleton<ILoggerFactory, LoggerFactory>());
                    services.TryAdd(ServiceDescriptor.Singleton(typeof(ILogger<>), typeof(Logger<>)));
                    services.Configure<ConnectionStrings>(config.GetSection("ConnectionStrings"));
                    var reduxConnectionString = config.GetConnectionString("ReduxConnectionString");
                    services.AddSingleton(new ReduxTableStorage<CertState>(reduxConnectionString));
                    services.AddSingleton(new ReduxTableStorage<UserState>(reduxConnectionString));
                    services.AddSingleton(new ReduxTableStorage<CounterState>(reduxConnectionString));
                })
                .ConfigureApplicationParts(parts =>
                    parts.AddApplicationPart(typeof(CounterGrain).Assembly).WithReferences())
                .Build();
            await host.StartAsync();
            return host;
        }
    }
}