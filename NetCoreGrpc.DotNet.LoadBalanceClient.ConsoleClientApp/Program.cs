using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using Grpc.Core;
using Grpc.Net.Client;
using Grpc.Net.Client.LoadBalancing.Policies;
using Grpc.Net.Client.LoadBalancing.ResolverPlugins;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NetCoreGrpc.LoadBalance.Proto;

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
                ResolverPlugin = GetGrpcResolverPlugin(),
                LoadBalancingPolicy = new RoundRobinPolicy()
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
            var isLocalEnvironment = bool.TryParse(Environment.GetEnvironmentVariable("IS_LOCAL_ENV"), out bool x) ? x : false;
            if (isLocalEnvironment)
            {
                return NullLoggerFactory.Instance;
            }
            return LoggerFactory.Create(x =>
            {
                x.AddConsole();
                x.SetMinimumLevel(LogLevel.Trace);
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
                        new GrpcNameResolutionResult("127.0.0.1", 8000)
                        {
                            IsLoadBalancer = false,
                        }
                    };
                });
            }
            else
            {
                return new DnsClientResolverPlugin();
            }
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
