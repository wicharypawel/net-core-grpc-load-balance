using k8s;
using k8s.Exceptions;
using k8s.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SimpleBalancer.App_Infrastructure.Options;
using SimpleBalancer.Services.Abstraction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleBalancer.Services.Implementation
{
    internal sealed class KubernetesEndpointWatcher : IEndpointWatcher, IDisposable
    {
        private readonly Kubernetes _k8sClient;
        private readonly BalancerOptions _options;
        private readonly ILogger _logger;
        private IReadOnlyList<EndpointEntry> _endpointEntries;

        public KubernetesEndpointWatcher(IWebHostEnvironment env,
            IOptions<BalancerOptions> options,
            ILogger<KubernetesEndpointWatcher> logger)
        {
            _options = options.Value;
            _logger = logger;
            _endpointEntries = Array.Empty<EndpointEntry>();
            try
            {
                _k8sClient = new Kubernetes(KubernetesClientConfiguration.InClusterConfig());
                InitializeWatcherAsync().Wait();
                _logger.LogDebug("Connected to K8s, watcher initialized");
            }
            catch (KubeConfigException x) when (PredefinedConfiguration.HasPredefinedConfiguration(x, env))
            {
                _k8sClient = null;
                _endpointEntries = PredefinedConfiguration.GetEndpointEntries(env);
                _logger.LogDebug($"Can not connect to K8s, using predefined configuration");
            }
        }

        public IReadOnlyList<EndpointEntry> GetEndpointEntries()
        {
            return new List<EndpointEntry>(_endpointEntries).AsReadOnly();
        }

        private Task InitializeWatcherAsync()
        {
            var service = _options.KubernetesServiceOption;
            return _k8sClient.WatchNamespacedEndpointsAsync(service.Name, service.Namespace, onEvent: onEvent);
        }

        private void onEvent(WatchEventType type, V1Endpoints endpoints)
        {
            _logger.LogDebug($"{type} detected {endpoints?.Metadata?.Name}");
            switch (type)
            {
                case WatchEventType.Added:
                case WatchEventType.Modified:
                    ModifiedEndpoints(endpoints); break;
                case WatchEventType.Deleted:
                case WatchEventType.Error:
                    _endpointEntries = new List<EndpointEntry>(); break;
                default: throw new InvalidOperationException();
            }
        }

        private void ModifiedEndpoints(V1Endpoints endpoints)
        {
            var list = new List<EndpointEntry>();
            foreach (var subset in endpoints.Subsets)
            {
                var port = subset.Ports.First().Port;
                foreach (var address in subset.Addresses)
                {
                    list.Add(new EndpointEntry(address.Ip, port));
                }
            }
            _endpointEntries = list;
        }

        public void Dispose()
        {
            _k8sClient?.Dispose();
        }
    }
}
