using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Lb.V1;
using Microsoft.Extensions.Options;
using NetCoreGrpc.MyGrpcLoadBalancer.App_Infrastructure.Options;
using System;
using System.Net;
using System.Threading.Tasks;

namespace NetCoreGrpc.MyGrpcLoadBalancer.Services
{
    public class LoadBalancerService : LoadBalancer.LoadBalancerBase
    {
        private readonly KubernetesEndpointWatcher _watcher;
        private readonly BalancerOptions _options;

        public LoadBalancerService(KubernetesEndpointWatcher watcher, IOptions<BalancerOptions> options)
        {
            _watcher = watcher;
            _options = options.Value;
        }

        public override async Task BalanceLoad(IAsyncStreamReader<LoadBalanceRequest> requestStream, IServerStreamWriter<LoadBalanceResponse> responseStream, ServerCallContext context)
        {
            Console.WriteLine("BalanceLoad start");
            while (await requestStream.MoveNext())
            {
                var requestType = requestStream.Current.LoadBalanceRequestTypeCase;
                if(requestType == LoadBalanceRequest.LoadBalanceRequestTypeOneofCase.None)
                {
                    Console.WriteLine("BalanceLoad none request - skip");
                    continue;
                }
                Console.WriteLine($"BalanceLoad next {requestType}");
                if (requestType == LoadBalanceRequest.LoadBalanceRequestTypeOneofCase.InitialRequest && !_options.IsIgnoringInitialRequest)
                {
                    var serviceName = requestStream.Current.InitialRequest.Name;
                    Console.WriteLine($"BalanceLoad initialRequest {serviceName}");
                    await responseStream.WriteAsync(new LoadBalanceResponse()
                    {
                        InitialResponse = new InitialLoadBalanceResponse()
                        {
                            ClientStatsReportInterval = Duration.FromTimeSpan(TimeSpan.FromSeconds(5)),
                            LoadBalancerDelegate = string.Empty
                        }
                    });
                }
                else
                {
                    var response = new LoadBalanceResponse();
                    var responseServerList = new ServerList();
                    foreach (var entry in _watcher.GetEndpointEntries())
                    {
                        Console.WriteLine($"BalanceLoad response {entry.Ip} {entry.Port}");
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
}
