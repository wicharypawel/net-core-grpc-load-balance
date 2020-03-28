using Grpc.Net.Client;
using Grpc.Net.Client.LoadBalancing.Policies;
using Grpc.Net.Client.LoadBalancing.ResolverPlugins;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NetCoreGrpc.LoadBalance.Proto;
using System;
using System.Collections.Generic;

namespace NetCoreGrpc.DotNet.LoadBalanceExternal.AspNetClientApp
{
    public sealed class Startup
    {
        public Startup(IConfiguration configuration)
        {
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddGrpcClient<Greeter.GreeterClient>(o =>
            {
                var target = Configuration["SERVICE_TARGET"];
                o.Address = new UriBuilder(target).Uri;
                o.ChannelOptionsActions.Add((options) =>
                {
                    options.ResolverPlugin = GetGrpcResolverPlugin();
                    options.LoadBalancingPolicy = new GrpclbPolicy();
                });
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        private static IGrpcResolverPlugin GetGrpcResolverPlugin()
        {
            var isLocalEnvironment = bool.TryParse(Environment.GetEnvironmentVariable("IS_LOCAL_ENV"), out bool x) ? x : false;
            if (isLocalEnvironment)
            {
                return new StaticResolverPlugin((uri) =>
                {
                    return new List<GrpcNameResolutionResult>()
                    {
                        new GrpcNameResolutionResult("127.0.0.1", 9000)
                        {
                            IsLoadBalancer = true,
                        }
                    };
                });
            }
            else
            {
                return new DnsClientResolverPlugin();
            }
        }
    }
}
