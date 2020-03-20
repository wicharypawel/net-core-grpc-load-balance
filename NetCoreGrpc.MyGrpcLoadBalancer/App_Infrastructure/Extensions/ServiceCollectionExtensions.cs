using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NetCoreGrpc.MyGrpcLoadBalancer.App_Infrastructure.Options;

namespace NetCoreGrpc.MyGrpcLoadBalancer.App_Infrastructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddBalancerOptions(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions<BalancerOptions>().Configure(options =>
            {
                if (bool.TryParse(configuration["SIMPLEBALANCER_IGNORE_INITIALREQUEST"], out bool isIgnoringInitialRequest))
                {
                    options.IsIgnoringInitialRequest = isIgnoringInitialRequest;
                }
                else
                {
                    options.IsIgnoringInitialRequest = false;
                }
            });
            return services;
        }
    }
}
