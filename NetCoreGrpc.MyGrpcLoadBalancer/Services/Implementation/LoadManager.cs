using System;
using System.Collections.Concurrent;

namespace NetCoreGrpc.MyGrpcLoadBalancer.Services.Implementation
{
    public sealed class LoadManager
    {
        private readonly ConcurrentDictionary<string, Guid> _loadBalanceTokensDictionary;

        public LoadManager()
        {
            _loadBalanceTokensDictionary = new ConcurrentDictionary<string, Guid>();
        }

        public string GetLoadBalanceToken(string serverAddress)
        {
            return _loadBalanceTokensDictionary.GetOrAdd(serverAddress, _ => Guid.NewGuid()).ToString();
        }
    }
}
