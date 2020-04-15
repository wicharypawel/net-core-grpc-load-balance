using Envoy.Api.V2;
using Envoy.Service.Discovery.V2;
using Grpc.Core;
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
            // kubectl port-forward -n istio-system service/istio-pilot 15010:15010
            // https://github.com/grpc/grpc-java/blob/master/xds/src/main/java/io/grpc/xds/XdsClientImpl.java
            var channel = new Channel("localhost:15010", ChannelCredentials.Insecure);
            var client = new AggregatedDiscoveryService.AggregatedDiscoveryServiceClient(channel);
            var connection = client.StreamAggregatedResources();
            // method sendXdsRequest
            await connection.RequestStream.WriteAsync(new DiscoveryRequest()
            {

                TypeUrl = ADS_TYPE_URL_LDS,
                ResourceNames = { },
                VersionInfo = string.Empty,
                ResponseNonce = string.Empty,
                Node = new Envoy.Api.V2.Core.Node()
                {
                    Id = "sidecar~192.168.0.1~xds.default~default.svc.cluster.local",
                }
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
                Node = new Envoy.Api.V2.Core.Node()
                {
                    Id = "sidecar~192.168.0.1~xds.default~default.svc.cluster.local",
                }
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
                Node = new Envoy.Api.V2.Core.Node()
                {
                    Id = "sidecar~192.168.0.1~xds.default~default.svc.cluster.local",
                }
            });
            await connection.ResponseStream.MoveNext(CancellationToken.None);
            discoveryResponse = connection.ResponseStream.Current;
            version = discoveryResponse.VersionInfo;
            nonce = discoveryResponse.Nonce;
            var clusters = discoveryResponse.Resources.Select(x => Cluster.Parser.ParseFrom(x.Value))
                .ToList();
            var myCluster = clusters
                .Where(x => x.Type == Cluster.Types.DiscoveryType.Eds)
                .Where(x => x.EdsClusterConfig.ServiceName == "outbound|8000||grpc-server.default.svc.cluster.local").FirstOrDefault();
            //////////////////////////////////////////
            await connection.RequestStream.WriteAsync(new DiscoveryRequest()
            {

                TypeUrl = ADS_TYPE_URL_EDS,
                ResourceNames = { myCluster.Name },
                VersionInfo = version,
                ResponseNonce = nonce,
                Node = new Envoy.Api.V2.Core.Node()
                {
                    Id = "sidecar~192.168.0.1~xds.default~default.svc.cluster.local",
                }
            });
            await connection.ResponseStream.MoveNext(CancellationToken.None);
            discoveryResponse = connection.ResponseStream.Current;
            version = discoveryResponse.VersionInfo;
            nonce = discoveryResponse.Nonce;
            var clusterLoadAssignments = discoveryResponse.Resources.Select(x => ClusterLoadAssignment.Parser.ParseFrom(x.Value))
                .ToList();
            connection.RequestStream.CompleteAsync().Wait();
            connection.Dispose();
            channel.ShutdownAsync().Wait();
        }
    }
}
