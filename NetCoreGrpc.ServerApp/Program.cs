using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.Extensions.Hosting;
using System;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;

namespace NetCoreGrpc.ServerApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            string? urlsEnvironmentVariable = Environment.GetEnvironmentVariable("ASPNETCORE_URLS");
            var httpPort = TryGetPortForScheme(urlsEnvironmentVariable, "http", out var httpPortValue) ? httpPortValue : 8000;
            var httpsPort = TryGetPortForScheme(urlsEnvironmentVariable, "https", out var httpsPortValue) ? httpsPortValue : 8001;
            return Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    webBuilder.ConfigureKestrel(options =>
                    {
                        options.Listen(IPAddress.Any, httpPort);
                        options.Listen(IPAddress.Any, httpsPort, listenOptions =>
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

        /// <summary>
        /// Try to get port for specified scheme out of string.
        /// </summary>
        /// <param name="variable">A string containing a port to find.</param>
        /// <param name="scheme">Scheme value eg. http.</param>
        /// <param name="port">Port value if the lookup succeeded, or zero if failed.</param>
        /// <returns>True if port was found successfully; otherwise, false.</returns>
        private static bool TryGetPortForScheme(string? variable, string scheme, out int port)
        {
            port = 0;
            if (variable == null)
            {
                return false;
            }
            var regex = new Regex(scheme + "://[^;]*:(?<portGroup>\\d{1,5})");
            var match = regex.Match(variable);
            var group = match.Groups["portGroup"];
            if (!match.Success || !group.Success)
            {
                return false;
            }
            if (int.TryParse(group.Value, out var portValue))
            {
                port = portValue;
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
