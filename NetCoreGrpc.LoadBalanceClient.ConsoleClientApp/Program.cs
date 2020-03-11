using System;
using System.Collections.Generic;
using Grpc.Core;

namespace NetCoreGrpc.LoadBalanceClient.ConsoleClientApp
{
    public class Program
    {
        public static void Main()
        {
            Console.WriteLine("This is just a code snippet, it is not a fully working example!!!");
            var channelOptions = new List<ChannelOption>();
            channelOptions.Add(new ChannelOption("grpc.lb_policy_name", "round_robin"));
            var channel = new Channel("http://test.app.com:5000", ChannelCredentials.Insecure, channelOptions);
            channel.ShutdownAsync().Wait();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
