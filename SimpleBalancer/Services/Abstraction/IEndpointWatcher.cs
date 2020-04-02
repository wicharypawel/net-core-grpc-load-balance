using System.Collections.Generic;

namespace SimpleBalancer.Services.Abstraction
{
    public interface IEndpointWatcher
    {
        public IReadOnlyList<EndpointEntry> GetEndpointEntries();
    }
}
