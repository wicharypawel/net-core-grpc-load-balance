using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NetCoreGrpc.MyGrpcLoadBalancer.App_Infrastructure.Extensions;
using NetCoreGrpc.MyGrpcLoadBalancer.App_Infrastructure.Options;
using NetCoreGrpc.MyGrpcLoadBalancer.Services;
using NetCoreGrpc.MyGrpcLoadBalancer.Services.Abstraction;
using NetCoreGrpc.MyGrpcLoadBalancer.Services.Implementation;

namespace NetCoreGrpc.MyGrpcLoadBalancer
{
    public sealed class Startup
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
            services.AddSingleton<IEndpointWatcher, KubernetesEndpointWatcher>();
            services.AddSingleton<ILoadManager, LoadManager>();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.ApplicationServices.GetRequiredService<IEndpointWatcher>(); // service warmup
            app.ApplicationServices.GetRequiredService<ILoadManager>(); // service warmup
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGrpcService<LoadBalancerService>();
                endpoints.MapGet("/configure/fallback", async context =>
                {
                    var options = context.RequestServices.GetService<IOptions<BalancerOptions>>();
                    options.Value.FallbackEnabled = !options.Value.FallbackEnabled;
                    await context.Response.WriteAsync($"Fallback set to {options.Value.FallbackEnabled}");
                });
                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Hello from SimpleBalancer for gRPC");
                });
            });
        }
    }
}
