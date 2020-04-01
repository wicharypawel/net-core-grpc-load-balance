using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Lb.V1;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetCoreGrpc.MyGrpcLoadBalancer.App_Infrastructure.Options;
using NetCoreGrpc.MyGrpcLoadBalancer.Services.Abstraction;
using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace NetCoreGrpc.MyGrpcLoadBalancer.Services
{
    public sealed class LoadBalancerService : LoadBalancer.LoadBalancerBase
    {
        private readonly IEndpointWatcher _watcher;
        private readonly BalancerOptions _options;
        private readonly ILogger _logger;
        private readonly ILoadManager _loadManager;

        public LoadBalancerService(IEndpointWatcher watcher, 
            IOptions<BalancerOptions> options,
            ILogger<LoadBalancerService> logger,
            ILoadManager loadManager)
        {
            _watcher = watcher;
            _options = options.Value;
            _logger = logger;
            _loadManager = loadManager;
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
            if (_options.ValidateServiceName && !_options.KubernetesServiceOption.ValidateServiceName(serviceName))
            {
                _logger.LogDebug($"Service name not configured for loadbalancer");
                throw new InvalidOperationException("Service name not configured for loadbalancer");
            }
            await responseStream.WriteAsync(new LoadBalanceResponse()
            {
                InitialResponse = new InitialLoadBalanceResponse()
                {
                    ClientStatsReportInterval = Duration.FromTimeSpan(_options.ClientStatsReportInterval),
                    LoadBalancerDelegate = string.Empty // deprecated https://github.com/grpc/grpc-proto/pull/78
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

            if (_options.FallbackEnabled)
            {
                await SendFallbackAsync(responseStream);
            }
            else
            {
                await SendServerListAsync(responseStream);
            }
        }

        private async Task SendServerListAsync(IServerStreamWriter<LoadBalanceResponse> responseStream)
        {
            var responseServerList = new ServerList();
            foreach (var entry in _watcher.GetEndpointEntries())
            {
                _logger.LogDebug($"BalanceLoad ServerList response {entry.Ip} {entry.Port}");
                responseServerList.Servers.Add(new Server()
                {
                    IpAddress = ByteString.CopyFrom(IPAddress.Parse(entry.Ip).GetAddressBytes()),
                    Port = entry.Port,
                    LoadBalanceToken = _options.EnableLoadBalanceTokens ? 
                        _loadManager.GetLoadBalanceToken(entry.Ip) : string.Empty
                });
            }
            await responseStream.WriteAsync(new LoadBalanceResponse() { ServerList = responseServerList });
        }

        private async Task SendFallbackAsync(IServerStreamWriter<LoadBalanceResponse> responseStream)
        {
            _logger.LogDebug($"BalanceLoad FallbackResponse response");
            await responseStream.WriteAsync(new LoadBalanceResponse() { FallbackResponse = new FallbackResponse() });
        }
    }
}
