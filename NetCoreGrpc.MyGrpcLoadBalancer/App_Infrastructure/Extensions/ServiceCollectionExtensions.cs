using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NetCoreGrpc.MyGrpcLoadBalancer.App_Infrastructure.Options;
using System;

namespace NetCoreGrpc.MyGrpcLoadBalancer.App_Infrastructure.Extensions
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
            });
            return services;
        }
    }
}
