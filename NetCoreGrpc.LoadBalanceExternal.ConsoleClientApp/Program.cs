using System;
using System.Collections.Generic;
using Grpc.Core;
using System.Threading;
using NetCoreGrpc.LoadBalance.Proto;

namespace NetCoreGrpc.LoadBalanceExternal.ConsoleClientApp
{
    public class Program
    {
        public static void Main()
        {
            var channelOptions = new List<ChannelOption>();
            channelOptions.Add(new ChannelOption("grpc.dns_enable_srv_queries", 1));

            var channelTarget = Environment.GetEnvironmentVariable("SERVICE_TARGET");
            var channel = new Channel(channelTarget, ChannelCredentials.Insecure, channelOptions);
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
    }
}
