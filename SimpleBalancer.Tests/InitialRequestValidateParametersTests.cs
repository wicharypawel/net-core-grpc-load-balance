using AutoFixture;
using Grpc.Core;
using Grpc.Lb.V1;
using Grpc.Net.Client;
using Microsoft.AspNetCore.TestHost;
using SimpleBalancer;
using SimpleBalancer.Tests.App_Infrastructure.ClassFixture;
using SimpleBalancer.Tests.App_Infrastructure.Extensions;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace NetCoreGrpcIntegrationTests.AspNetCoreServerApp.Tests
{
    public sealed class InitialRequestValidateParametersTests : IClassFixture<CustomWebApplicationFactory<Startup>>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory<Startup> _factory;
        private readonly Fixture _fixture;
        private readonly GrpcChannel _channel;

        public InitialRequestValidateParametersTests(CustomWebApplicationFactory<Startup> factory)
        {
            _factory = factory;
            _client = _factory.WithWebHostBuilder(builder =>
            {
                builder.UseSetting("SIMPLEBALANCER_CLIENT_STATS_REPORT_INTERVAL", "00:00:05");
                builder.UseSetting("SIMPLEBALANCER_VALIDATE_SERVICE_NAME", "true");
                builder.UseSetting("SIMPLEBALANCER_K8S_SERVICE", "name=test-service-name;namespace=default");
                builder.ConfigureTestServices(services =>
                {
                    // ConfigureServices of DI
                });
            }).CreateClientForGrpc();
            _channel = GrpcChannel.ForAddress("http://randomAddress:1234", new GrpcChannelOptions()
            {
                HttpClient = _client
            });
            _fixture = new Fixture();
        }

        [Trait("Category", "Integration")]
        [Fact]
        public async Task ForWrongServiceName_UsingBalanceLoadValidateServiceName_ThrowException()
        {
            var balancerClient = new LoadBalancer.LoadBalancerClient(_channel);
            var initialRequest = new InitialLoadBalanceRequest() { Name = "wrong-service-name" };
            var request = new LoadBalanceRequest() { InitialRequest = initialRequest };

            // Act
            // Assert
            using var balancingStreaming = balancerClient.BalanceLoad();
            await balancingStreaming.RequestStream.WriteAsync(request);
            await balancingStreaming.RequestStream.CompleteAsync();
            var exception = await Assert.ThrowsAsync<RpcException>(async () =>
            {
                var _ = await balancingStreaming.ResponseStream.MoveNext();
            });
            Assert.Equal(StatusCode.Unknown, exception.StatusCode);
        }

        [Trait("Category", "Integration")]
        [Fact]
        public async Task ForCorrectServiceName_UsingBalanceLoadValidateServiceName_ReturnResponse()
        {
            var balancerClient = new LoadBalancer.LoadBalancerClient(_channel);
            var initialRequest = new InitialLoadBalanceRequest() { Name = "test-service-name" };
            var request = new LoadBalanceRequest() { InitialRequest = initialRequest };

            // Act
            using var balancingStreaming = balancerClient.BalanceLoad();
            await balancingStreaming.RequestStream.WriteAsync(request);
            await balancingStreaming.RequestStream.CompleteAsync();
            var hasFirstElement = await balancingStreaming.ResponseStream.MoveNext();
            var hasSecondElement = await balancingStreaming.ResponseStream.MoveNext();

            // Assert
            Assert.True(hasFirstElement);
            Assert.True(hasSecondElement);
        }

        [Trait("Category", "Integration")]
        [Fact]
        public async Task ForInitialRequest_UsingBalanceLoadCheckReportInterval_ReturnFiveSeconds()
        {
            var balancerClient = new LoadBalancer.LoadBalancerClient(_channel);
            var initialRequest = new InitialLoadBalanceRequest() { Name = "test-service-name" };
            var request = new LoadBalanceRequest() { InitialRequest = initialRequest };

            // Act
            using var balancingStreaming = balancerClient.BalanceLoad();
            await balancingStreaming.RequestStream.WriteAsync(request);
            await balancingStreaming.RequestStream.CompleteAsync();
            var hasFirstElement = await balancingStreaming.ResponseStream.MoveNext();
            var response = balancingStreaming.ResponseStream.Current;
            var hasSecondElement = await balancingStreaming.ResponseStream.MoveNext();

            // Assert
            Assert.True(hasFirstElement);
            Assert.True(hasSecondElement);
            Assert.Equal(LoadBalanceResponse.LoadBalanceResponseTypeOneofCase.InitialResponse, response.LoadBalanceResponseTypeCase);
            Assert.NotNull(response.InitialResponse);
            Assert.Equal(TimeSpan.FromSeconds(5), response.InitialResponse.ClientStatsReportInterval.ToTimeSpan());
        }
    }
}
