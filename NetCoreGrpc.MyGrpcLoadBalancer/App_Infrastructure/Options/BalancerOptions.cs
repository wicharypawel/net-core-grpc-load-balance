using System;
using System.Collections.Generic;
using System.Linq;

namespace NetCoreGrpc.MyGrpcLoadBalancer.App_Infrastructure.Options
{
    public sealed class BalancerOptions
    {
        public TimeSpan ClientStatsReportInterval { get; set; }
        public bool EnableLoadBalanceTokens { get; set; }
        public KubernetesServiceOption KubernetesServiceOption { get; set; }
        public bool ValidateServiceName { get; set; }
        public bool FallbackEnabled { get; set; }
    }

    public sealed class KubernetesServiceOption
    {
        public string Namespace { get; set; }
        public string Name { get; set; }
        public List<string> AliasListForClients { get; } = new List<string>();

        internal bool ValidateServiceName(string serviceName)
        {
            return ValidateServiceNameCore(serviceName) || 
                ValidateServiceNameCore(new UriBuilder($"dns://{serviceName}").Uri.Host);
        }

        private bool ValidateServiceNameCore(string serviceName)
        {
            return Name.Equals(serviceName, StringComparison.OrdinalIgnoreCase) || 
                AliasListForClients.Any(x => x.Equals(serviceName, StringComparison.OrdinalIgnoreCase));
        }
    }
}
