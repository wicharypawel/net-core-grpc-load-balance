using Grpc.Core;
using Microsoft.AspNetCore.Mvc;
using NetCoreGrpc.LoadBalance.Proto;
using System.Text;

namespace NetCoreGrpc.DotNet.LoadBalanceExternal.AspNetClientApp.Controllers
{
    [ApiController]
    [Route("api/Greeter")]
    public class GreeterController : ControllerBase
    {
        private readonly Greeter.GreeterClient _client;

        public GreeterController(Greeter.GreeterClient client)
        {
            _client = client;
        }

        [HttpGet("sayhello")]
        public IActionResult SayHello()
        {
            var result = new StringBuilder();
            var user = "Pawel";
            for (int i = 0; i < 10; i++)
            {
                try
                {
                    var reply = _client.SayHello(new HelloRequest { Name = user });
                    result.AppendLine("Greeting: " + reply.Message);
                }
                catch (RpcException e)
                {
                    result.AppendLine("Error invoking: " + e.Status);
                }
            }
            return Ok($"gRPC returned:\n{result}");
        }
    }
}
