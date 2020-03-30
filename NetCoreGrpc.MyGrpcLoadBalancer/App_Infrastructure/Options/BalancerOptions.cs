using System;

namespace NetCoreGrpc.MyGrpcLoadBalancer.App_Infrastructure.Options
{
    public sealed class BalancerOptions
    {
        public TimeSpan ClientStatsReportInterval { get; set; }
    }
}
