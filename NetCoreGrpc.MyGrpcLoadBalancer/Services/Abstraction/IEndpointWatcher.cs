using System.Collections.Generic;

namespace NetCoreGrpc.MyGrpcLoadBalancer.Services.Abstraction
{
    public interface IEndpointWatcher
    {
        public IReadOnlyList<EndpointEntry> GetEndpointEntries();
    }
}
