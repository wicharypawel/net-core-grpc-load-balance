using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Net;

namespace SimpleBalancer
{
    public sealed class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureLogging(loggingBuilder =>
                {
                    loggingBuilder.ClearProviders();
                    loggingBuilder.AddConsole();
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    webBuilder.ConfigureKestrel(kestrelOptions =>
                    {
                        kestrelOptions.Listen(IPAddress.Any, 9000, grpcEndpoint =>
                        {
                            grpcEndpoint.Protocols = HttpProtocols.Http2;
                        });
                        kestrelOptions.Listen(IPAddress.Any, 9100, webEndpoint =>
                        {
                            webEndpoint.Protocols = HttpProtocols.Http1AndHttp2;
                        });
                    });
                });
    }
}
