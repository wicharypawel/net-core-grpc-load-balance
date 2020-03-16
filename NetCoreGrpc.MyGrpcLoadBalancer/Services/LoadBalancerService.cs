using Google.Protobuf;
using Grpc.Core;
using Grpc.Lb.V1;
using System;
using System.Net;
using System.Threading.Tasks;

namespace NetCoreGrpc.MyGrpcLoadBalancer.Services
{
    public class LoadBalancerService : LoadBalancer.LoadBalancerBase
    {
        private readonly KubernetesEndpointWatcher _watcher;

        public LoadBalancerService(KubernetesEndpointWatcher watcher)
        {
            _watcher = watcher;
        }

        public override async Task BalanceLoad(IAsyncStreamReader<LoadBalanceRequest> requestStream, IServerStreamWriter<LoadBalanceResponse> responseStream, ServerCallContext context)
        {
            Console.WriteLine("Loadbalancer Request");
            while (await requestStream.MoveNext())
            {
                var response = new LoadBalanceResponse();
                var responseServerList = new ServerList();
                foreach (var entry in _watcher.GetEndpointEntries())
                {
                    Console.WriteLine($"Loadbalancer Response {entry.Ip} {entry.Port}");
                    responseServerList.Servers.Add(new Server()
                    {
                        IpAddress = ByteString.CopyFrom(IPAddress.Parse(entry.Ip).GetAddressBytes()),
                        Port = entry.Port
                    });
                }
                response.ServerList = responseServerList;
                await responseStream.WriteAsync(response);
            }
        }
    }
}
