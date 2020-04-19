using Grpc.Net.Client.LoadBalancing;
using Grpc.Net.Client.LoadBalancing.Extensions;
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
            LoadBalancingPolicyRegistry.GetDefaultRegistry().RegisterGrpclb();
            services.AddGrpcClient<Greeter.GreeterClient>(o =>
            {
                var target = Configuration["SERVICE_TARGET"];
                o.Address = new UriBuilder(target).Uri;
                o.ChannelOptionsActions.Add((options) =>
                {
                    options.ResolverPlugin = GetGrpcResolverPlugin();
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
                    var hosts = new List<GrpcHostAddress>()
                    {
                        new GrpcHostAddress("127.0.0.1", 9000)
                        {
                            IsLoadBalancer = true,
                        }
                    };
                    var config = GrpcServiceConfigOrError.FromConfig(GrpcServiceConfig.Create("grpclb", "pick_first"));
                    return new GrpcNameResolutionResult(hosts, config, GrpcAttributes.Empty);
                });
            }
            else
            {
                return new DnsClientResolverPlugin(new DnsClientResolverPluginOptions()
                {
                    EnableSrvGrpclb = true
                });
            }
        }
    }
}
