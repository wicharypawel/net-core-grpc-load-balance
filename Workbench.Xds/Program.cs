using Envoy.Api.V2;
using Envoy.Api.V2.Core;
using Envoy.Service.Discovery.V2;
using Grpc.Core;
using Grpc.Net.Client;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Workbench.Xds
{
    internal class Program
    {
        private static string ADS_TYPE_URL_LDS = "type.googleapis.com/envoy.api.v2.Listener";
        private static string ADS_TYPE_URL_RDS = "type.googleapis.com/envoy.api.v2.RouteConfiguration";
        private static string ADS_TYPE_URL_CDS = "type.googleapis.com/envoy.api.v2.Cluster";
        private static string ADS_TYPE_URL_EDS = "type.googleapis.com/envoy.api.v2.ClusterLoadAssignment";

        private static string version = "";
        private static string nonce = "";

        static void Main(string[] args)
        {
            MainAsync(args).Wait();
        }

        private static async Task MainAsync(string[] args)
        {
            var node = new Node()
            {
                Id = "sidecar~192.168.0.1~xds.default~default.svc.cluster.local",
                Cluster = "",
                UserAgentName = "grpc-dotnet",
                UserAgentVersion = "1.0.0",
                Locality = new Locality()
                {
                    Region = "local-k8s-cluster",
                    Zone = "a"
                },
                ClientFeatures = { "envoy.lb.does_not_support_overprovisioning" }
            };
            var serviceName = "outbound|8000||grpc-server.default.svc.cluster.local";
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            // kubectl port-forward -n istio-system service/istio-pilot 15010:15010
            // https://github.com/grpc/grpc-java/blob/master/xds/src/main/java/io/grpc/xds/XdsClientImpl.java
            var channel = GrpcChannel.ForAddress("http://localhost:15010", new GrpcChannelOptions() { Credentials = ChannelCredentials.Insecure });
            var client = new AggregatedDiscoveryService.AggregatedDiscoveryServiceClient(channel);
            var connection = client.StreamAggregatedResources();
            // method sendXdsRequest
            await connection.RequestStream.WriteAsync(new DiscoveryRequest()
            {

                TypeUrl = ADS_TYPE_URL_LDS,
                ResourceNames = { },
                VersionInfo = string.Empty,
                ResponseNonce = string.Empty,
                Node = node
            });
            // method onNext
            await connection.ResponseStream.MoveNext(CancellationToken.None);
            // class ListenerResourceFetchTimeoutTask (the same file)
            var discoveryResponse = connection.ResponseStream.Current;
            version = discoveryResponse.VersionInfo;
            nonce = discoveryResponse.Nonce;
            // method handleLdsResponseForListener
            var listeners = discoveryResponse.Resources.Select(x => Listener.Parser.ParseFrom(x.Value)).ToList();
            var mylistenerList = listeners.Where(x => x.Address.SocketAddress.PortValue == 8000).ToList();
            if (mylistenerList.Count > 1)
            {
                throw new InvalidOperationException("One listener expected");
            }
            var mylistener = mylistenerList.First();
            //////////////////////////////////////////
            await connection.RequestStream.WriteAsync(new DiscoveryRequest()
            {

                TypeUrl = ADS_TYPE_URL_RDS,
                ResourceNames = { mylistener.Name },
                VersionInfo = version,
                ResponseNonce = nonce,
                Node = node
            });
            await connection.ResponseStream.MoveNext(CancellationToken.None);
            discoveryResponse = connection.ResponseStream.Current;
            version = discoveryResponse.VersionInfo;
            nonce = discoveryResponse.Nonce;
            var routeConfigurations = discoveryResponse.Resources.Select(x => RouteConfiguration.Parser.ParseFrom(x.Value))
                .ToList();
            //////////////////////////////////////////
            await connection.RequestStream.WriteAsync(new DiscoveryRequest()
            {

                TypeUrl = ADS_TYPE_URL_CDS,
                ResourceNames = { },
                VersionInfo = version,
                ResponseNonce = nonce,
                Node = node
            });
            await connection.ResponseStream.MoveNext(CancellationToken.None);
            discoveryResponse = connection.ResponseStream.Current;
            version = discoveryResponse.VersionInfo;
            nonce = discoveryResponse.Nonce;
            var clusters = discoveryResponse.Resources.Select(x => Cluster.Parser.ParseFrom(x.Value))
                .ToList();
            var cluster = clusters
                .Where(x => x.Type == Cluster.Types.DiscoveryType.Eds)
                .Where(x => x?.EdsClusterConfig?.EdsConfig != null)
                .Where(x => x.LbPolicy == Cluster.Types.LbPolicy.RoundRobin)
                .Where(x => x?.Name.Contains(serviceName, StringComparison.OrdinalIgnoreCase) ?? false).First();
            //////////////////////////////////////////
            var edsClusterName = cluster.EdsClusterConfig?.ServiceName ?? cluster.Name;
            await connection.RequestStream.WriteAsync(new DiscoveryRequest()
            {

                TypeUrl = ADS_TYPE_URL_EDS,
                ResourceNames = { edsClusterName },
                VersionInfo = version,
                ResponseNonce = nonce,
                Node = node
            });
            await connection.ResponseStream.MoveNext(CancellationToken.None);
            discoveryResponse = connection.ResponseStream.Current;
            version = discoveryResponse.VersionInfo;
            nonce = discoveryResponse.Nonce;
            var clusterLoadAssignments = discoveryResponse.Resources.Select(x => ClusterLoadAssignment.Parser.ParseFrom(x.Value))
                .ToList();
            var clusterLoadAssignment = clusterLoadAssignments
                .Where(x => x.Endpoints.Count != 0)
                .Where(x => x.Endpoints[0].LbEndpoints.Count != 0)
                .First();
            connection.RequestStream.CompleteAsync().Wait();
            connection.Dispose();
            channel.ShutdownAsync().Wait();
        }
    }
}
