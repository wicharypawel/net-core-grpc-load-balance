using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SimpleBalancer.App_Infrastructure.Options;
using System;
using System.Linq;

namespace SimpleBalancer.App_Infrastructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddBalancerOptions(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions<BalancerOptions>().Configure(options =>
            {
                if (TimeSpan.TryParse(configuration["SIMPLEBALANCER_CLIENT_STATS_REPORT_INTERVAL"], out TimeSpan clientStatsReportInterval))
                {
                    options.ClientStatsReportInterval = clientStatsReportInterval;
                }
                else
                {
                    options.ClientStatsReportInterval = TimeSpan.FromSeconds(10);
                }

                if (bool.TryParse(configuration["SIMPLEBALANCER_ENABLE_LOADBALANCE_TOKENS"], out bool enableLoadBalanceTokens))
                {
                    options.EnableLoadBalanceTokens = enableLoadBalanceTokens;
                }
                else
                {
                    options.EnableLoadBalanceTokens = true;
                }

                if (TryParseKubernetesServiceOption(configuration[$"SIMPLEBALANCER_K8S_SERVICE"], out var kubernetesServiceOption))
                {
                    options.KubernetesServiceOption = kubernetesServiceOption;
                }
                else
                {
                    throw new ArgumentException("Missing configuration for k8s services (env:SIMPLEBALANCER_K8S_SERVICE)");
                }

                if (bool.TryParse(configuration["SIMPLEBALANCER_VALIDATE_SERVICE_NAME"], out bool validateServiceName))
                {
                    options.ValidateServiceName = validateServiceName;
                }
                else
                {
                    options.ValidateServiceName = false;
                }
                options.FallbackEnabled = false;
            });
            return services;
        }

        private static bool TryParseKubernetesServiceOption(string value, out KubernetesServiceOption kubernetesServiceOption)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                kubernetesServiceOption = null;
                return false;
            }
            try
            {
                var stringTuples = value.Split(";");
                var result = new KubernetesServiceOption()
                {
                    Name = stringTuples.First(x => x.StartsWith("name=")).Substring(5),
                    Namespace = stringTuples.First(x => x.StartsWith("namespace=")).Substring(10),
                };
                foreach (var stringTuple in stringTuples.Where(x => x.StartsWith("alias=")))
                {
                    result.AliasListForClients.Add(stringTuple.Substring(6));
                }
                kubernetesServiceOption = result;
                return true;
            }
            catch (Exception)
            {
                kubernetesServiceOption = null;
                return false;
            }
        }
    }
}
