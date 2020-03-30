using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Lb.V1;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetCoreGrpc.MyGrpcLoadBalancer.App_Infrastructure.Options;
using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace NetCoreGrpc.MyGrpcLoadBalancer.Services
{
    public class LoadBalancerService : LoadBalancer.LoadBalancerBase
    {
        private readonly KubernetesEndpointWatcher _watcher;
        private readonly BalancerOptions _options;
        private readonly ILogger _logger;

        public LoadBalancerService(KubernetesEndpointWatcher watcher, 
            IOptions<BalancerOptions> options,
            ILogger<LoadBalancerService> logger)
        {
            _watcher = watcher;
            _options = options.Value;
            _logger = logger;
        }

        public override async Task BalanceLoad(IAsyncStreamReader<LoadBalanceRequest> requestStream, 
            IServerStreamWriter<LoadBalanceResponse> responseStream, ServerCallContext context)
        {
            _logger.LogDebug("BalanceLoad start");
            while (await requestStream.MoveNext())
            {
                var requestType = requestStream.Current.LoadBalanceRequestTypeCase;
                _logger.LogDebug($"BalanceLoad next {requestType}, request received at {DateTime.UtcNow}");
                switch (requestType)
                {
                    case LoadBalanceRequest.LoadBalanceRequestTypeOneofCase.None:
                        _logger.LogDebug("BalanceLoad none request - skip");
                        continue;
                    case LoadBalanceRequest.LoadBalanceRequestTypeOneofCase.InitialRequest:
                        await ProcessInitialRequestAsync(requestStream, responseStream);
                        break;
                    case LoadBalanceRequest.LoadBalanceRequestTypeOneofCase.ClientStats:
                        await ProcessClientStatsAsync(requestStream, responseStream);
                        break;
                    default:
                        _logger.LogDebug("BalanceLoad request unknown - skip");
                        continue;
                }
            }
        }

        private async Task ProcessInitialRequestAsync(IAsyncStreamReader<LoadBalanceRequest> requestStream,
            IServerStreamWriter<LoadBalanceResponse> responseStream)
        {
            var serviceName = requestStream.Current.InitialRequest.Name;
            _logger.LogDebug($"BalanceLoad initialRequest {serviceName}");
            await responseStream.WriteAsync(new LoadBalanceResponse()
            {
                InitialResponse = new InitialLoadBalanceResponse()
                {
                    ClientStatsReportInterval = Duration.FromTimeSpan(_options.ClientStatsReportInterval),
                    LoadBalancerDelegate = string.Empty
                }
            });
            await SendServerListAsync(responseStream);
        }

        private async Task ProcessClientStatsAsync(IAsyncStreamReader<LoadBalanceRequest> requestStream,
            IServerStreamWriter<LoadBalanceResponse> responseStream)
        {
            var clientStats = requestStream.Current.ClientStats;
            _logger.LogDebug($"BalanceLoad ClientStats received {JsonSerializer.Serialize(clientStats)}");

            //TODO store and analyse client stats

            await SendServerListAsync(responseStream);
        }

        private async Task SendServerListAsync(IServerStreamWriter<LoadBalanceResponse> responseStream)
        {
            var responseServerList = new ServerList();
            foreach (var entry in _watcher.GetEndpointEntries())
            {
                _logger.LogDebug($"BalanceLoad response {entry.Ip} {entry.Port}");
                responseServerList.Servers.Add(new Server()
                {
                    IpAddress = ByteString.CopyFrom(IPAddress.Parse(entry.Ip).GetAddressBytes()),
                    Port = entry.Port
                });
            }
            await responseStream.WriteAsync(new LoadBalanceResponse() { ServerList = responseServerList });
        }
    }
}
