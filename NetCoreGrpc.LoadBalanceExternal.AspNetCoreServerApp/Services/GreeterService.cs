using System;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using NetCoreGrpc.LoadBalance.Proto;

namespace NetCoreGrpc.LoadBalanceExternal.AspNetCoreServerApp.Services
{
    public class GreeterService : Greeter.GreeterBase
    {
        private readonly ILogger<GreeterService> _logger;
        public GreeterService(ILogger<GreeterService> logger)
        {
            _logger = logger;
        }

        public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
        {
            return Task.FromResult(new HelloReply
            {
                Message = $"Hello {request.Name} (Backend IP: {Environment.GetEnvironmentVariable("MY_POD_IP") ?? "not found" })"
            });
        }
    }
}
