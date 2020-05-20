using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using NetCoreGrpc.LoadBalance.Proto;

namespace NetCoreGrpc.ServerApp.Services
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
            var port = context.GetHttpContext().Connection.LocalPort;
            return Task.FromResult(new HelloReply
            {
                Message = $"Hello {request.Name} (Backend IP: { GetLocalIPAddress() ?? "addres-not-found" }:{port})"
            });
        }

        private static string? GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            return null;
        }
    }
}
