using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.Extensions.Hosting;
using System.Net;
using System.Security.Cryptography.X509Certificates;

namespace NetCoreGrpc.ServerApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    webBuilder.ConfigureKestrel(options =>
                    {
                        options.Listen(IPAddress.Any, 8000);
                        options.Listen(IPAddress.Any, 8001, listenOptions =>
                        {
                            listenOptions.UseHttps(options =>
                            {
                                options.ClientCertificateMode = ClientCertificateMode.NoCertificate;
                                options.ServerCertificate = new X509Certificate2("sample_server_certificate.pfx", "7f7xusFpVgMFsFaFC37hQeeGyDDfRcWM");
                            });
                        });
                    });
                });
    }
}
