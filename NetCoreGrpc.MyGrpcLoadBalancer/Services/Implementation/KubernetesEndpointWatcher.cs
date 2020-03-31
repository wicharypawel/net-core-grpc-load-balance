using k8s;
using k8s.Exceptions;
using k8s.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NetCoreGrpc.MyGrpcLoadBalancer.Services.Implementation
{
    public sealed class KubernetesEndpointWatcher : IDisposable
    {
        private readonly Kubernetes _k8sClient;
        private IReadOnlyList<EndpointEntry> _endpointEntries;

        public KubernetesEndpointWatcher(IWebHostEnvironment hostEnvironment)
        {
            _endpointEntries = Array.Empty<EndpointEntry>();
            try
            {
                var config = KubernetesClientConfiguration.InClusterConfig();
                _k8sClient = new Kubernetes(config);
                _k8sClient.WatchNamespacedEndpointsAsync("grpc-server", "default", onEvent: onEvent);
            }
            catch (KubeConfigException x) when (x.Message.StartsWith("unable to load in-cluster configuration"))
            {
                if (hostEnvironment.IsDevelopment())
                {
                    _endpointEntries = new EndpointEntry[]
                    {
                        new EndpointEntry()
                        {
                            Ip = "127.0.0.1",
                            Port = 8000
                        }
                    };
                }
                else
                {
                    throw;
                }
            }
        }

        public IReadOnlyList<EndpointEntry> GetEndpointEntries()
        {
            return new List<EndpointEntry>(_endpointEntries).AsReadOnly();
        }

        private void onEvent(WatchEventType type, V1Endpoints endpoint)
        {
            switch (type)
            {
                case WatchEventType.Added:
                case WatchEventType.Modified:
                    ModifiedEndpoints(endpoint); break;
                case WatchEventType.Deleted:
                case WatchEventType.Error:
                    _endpointEntries = new List<EndpointEntry>(); break;
                default: throw new InvalidOperationException();
            }

        }

        private void ModifiedEndpoints(V1Endpoints endpoint)
        {
            var list = new List<EndpointEntry>();
            foreach (var subset in endpoint.Subsets)
            {
                var port = subset.Ports.First().Port;
                foreach (var address in subset.Addresses)
                {
                    list.Add(new EndpointEntry() { Ip = address.Ip, Port = port });
                }
            }
            _endpointEntries = list;
        }

        public void Dispose()
        {
            _k8sClient?.Dispose();
        }
    }

    public sealed class EndpointEntry
    {
        public string Ip { get; set; }
        public int Port { get; set; }
    }
}
