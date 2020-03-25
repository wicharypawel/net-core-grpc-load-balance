using System;
using Grpc.Core;
using System.Threading;
using NetCoreGrpc.LoadBalance.Proto;
using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using Grpc.Net.Client.LoadBalancing.Policies;
using Grpc.Net.Client.LoadBalancing.ResolverPlugins;
using System.Net;

namespace NetCoreGrpc.DotNet.LoadBalanceExternal.ConsoleClientApp
{
    public class Program
    {
        public static void Main()
        {
            const int defaultDnsPort = 53;
            var factory = LoggerFactory.Create(x =>
            {
                x.AddConsole();
                x.SetMinimumLevel(LogLevel.Trace);
            });
            var channelOptions = new GrpcChannelOptions()
            {
                //LoggerFactory = factory,
                HttpClient = CreateGrpcHttpClient(true),
                ResolverPlugin = new DnsClientResolverPlugin(new DnsClientResolverPluginOptions()
                {
                    NameServers = new IPEndPoint[] { new IPEndPoint(Dns.GetHostAddresses("custom-dns")[0], defaultDnsPort) }
                }),
                LoadBalancingPolicy = new GrpclbPolicy()
            };
            var channelTarget = Environment.GetEnvironmentVariable("SERVICE_TARGET");
            var channel = GrpcChannel.ForAddress("http://" + channelTarget, channelOptions);
            var client = new Greeter.GreeterClient(channel);
            var user = "Pawel";
            for (int i = 0; i < 10000; i++)
            {
                try
                {
                    var reply = client.SayHello(new HelloRequest { Name = user });
                    Console.WriteLine("Greeting: " + reply.Message);
                }
                catch (RpcException e)
                {
                    Console.WriteLine("Error invoking: " + e.Status);
                }
                Thread.Sleep(1000);
            }
            channel.ShutdownAsync().Wait();
            Console.WriteLine();
        }

        private static HttpClient CreateGrpcHttpClient(bool acceptSelfSignedCertificate = false)
        {
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            if (acceptSelfSignedCertificate)
            {
                var handler = new HttpClientHandler();
                handler.ServerCertificateCustomValidationCallback = (msg, cert, chain, errors) => true;
                var httpClient = new HttpClient(handler);
                httpClient.Timeout = Timeout.InfiniteTimeSpan;
                return httpClient;
            }
            else
            {
                var httpClient = new HttpClient();
                httpClient.Timeout = Timeout.InfiniteTimeSpan;
                return httpClient;
            }
        }
    }
}
