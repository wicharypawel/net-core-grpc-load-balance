using Grpc.Core;
using Grpc.Net.Client;
using Grpc.Net.Client.LoadBalancing;
using Microsoft.Extensions.Logging;
using NetCoreGrpc.LoadBalance.Proto;
using System;
using System.Net.Http;
using System.Threading;

namespace NetCoreGrpc.DotNet.LoadBalanceClient.ConsoleClientApp
{
    public class Program
    {
        public static void Main()
        {
            var channelOptions = new GrpcChannelOptions()
            {
                LoggerFactory = GetConsoleLoggerFactory(),
                HttpClient = CreateGrpcHttpClient(acceptSelfSignedCertificate: true),
                DefaultLoadBalancingPolicy = GetLoadBalancingPolicyName(),
                Attributes = GrpcAttributes.Builder.NewBuilder().Add(GrpcAttributesConstants.DnsResolverPeriodicResolutionSeconds, 30).Build()
            };
            var channelTarget = Environment.GetEnvironmentVariable("SERVICE_TARGET");
            var channel = GrpcChannel.ForAddress(channelTarget, channelOptions);
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

        private static ILoggerFactory GetConsoleLoggerFactory()
        {
            return LoggerFactory.Create(x =>
            {
                x.AddConsole();
                x.SetMinimumLevel(LogLevel.Trace);
            });
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

        private static string GetLoadBalancingPolicyName()
        {
            var loadBalancingPolicyName = Environment.GetEnvironmentVariable("LOAD_BALANCING_POLICY");
            return loadBalancingPolicyName == null ? "pick_first" : loadBalancingPolicyName;
        }
    }
}
