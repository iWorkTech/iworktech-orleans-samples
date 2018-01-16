using System;
using iWorkTech.Orleans.Interfaces;
using iWorkTech.Orleans.Web.Core.Hub;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Orleans;
using Orleans.Runtime.Configuration;

namespace iWorkTech.Orleans.Web.Core
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Adds a default in-memory implementation of IDistributedCache.
            services.AddDistributedMemoryCache();

            services.AddSession(options =>
            {
                // Set a short timeout for easy testing.
                options.IdleTimeout = TimeSpan.FromSeconds(10);
                options.Cookie.HttpOnly = true;
            });

            services.AddSession();

            services.AddMvc();

            services.AddCors();

            services.AddSignalR();
            //.AddOrleans();

            services.AddSingleton<IGrainFactory>(sp =>
            {
                var config = ClientConfiguration.LocalhostSilo()
                    .AddSignalR();
                var client = new ClientBuilder()
                    .UseConfiguration(config)
                    .ConfigureApplicationParts(parts =>
                        parts.AddApplicationPart(typeof(IPlayerGrain).Assembly).WithReferences())
                    .Build();
                client.Connect().Wait();

                return client;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseSession();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
                app.UseSignalR(routes =>
                {
                    routes.MapHub<LocationHub>("/location");
                    routes.MapHub<ChatHub>("/chat");
                    routes.MapHub<DrawHub>("/draw");
                    routes.MapHub<StreamingHub>("/streaming");
                });

                app.ApplicationServices.GetService<IGrainFactory>();

                // Shows UseCors with CorsPolicyBuilder.
                app.UseCors(builder =>
                    builder.WithOrigins("*"));
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    "default",
                    "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}