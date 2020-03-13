using System;
using System.Collections.Generic;
using System.Threading;
using Grpc.Core;
using NetCoreGrpc.LoadBalance.Proto;

namespace NetCoreGrpc.LoadBalanceClient.ConsoleClientApp
{
    public class Program
    {
        public static void Main()
        {
            var channelOptions = new List<ChannelOption>();
            channelOptions.Add(new ChannelOption("grpc.lb_policy_name", "round_robin"));
            var channel = new Channel("greeter-server.default.svc.cluster.local:8000", ChannelCredentials.Insecure, channelOptions);
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
