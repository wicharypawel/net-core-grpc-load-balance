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
            services.AddGrpcClient<Greeter.GreeterClient>(o =>
            {
                EnsureLoadAssembly.Load();
                var target = Configuration["SERVICE_TARGET"];
                o.Address = new UriBuilder(target).Uri;
                o.ChannelOptionsActions.Add((options) =>
                {
                    options.Attributes = GetAttributes();
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

        private static GrpcAttributes GetAttributes()
        {
            return new GrpcAttributes(new Dictionary<string, object>()
            {
                { GrpcAttributesLbConstants.DnsResolverOptions, new DnsClientResolverPluginOptions(){ EnableSrvGrpclb = true } }
            });
        }
    }
}
