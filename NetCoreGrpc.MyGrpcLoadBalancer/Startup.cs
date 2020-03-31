using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NetCoreGrpc.MyGrpcLoadBalancer.App_Infrastructure.Extensions;
using NetCoreGrpc.MyGrpcLoadBalancer.Services;
using NetCoreGrpc.MyGrpcLoadBalancer.Services.Implementation;

namespace NetCoreGrpc.MyGrpcLoadBalancer
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddBalancerOptions(Configuration);
            services.AddGrpc();
            services.AddSingleton<KubernetesEndpointWatcher>();
            services.AddSingleton<LoadManager>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.ApplicationServices.GetService<KubernetesEndpointWatcher>();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGrpcService<LoadBalancerService>();
                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
                });
            });
        }
    }
}
