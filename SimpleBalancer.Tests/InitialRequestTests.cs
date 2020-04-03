using AutoFixture;
using Grpc.Core;
using Grpc.Lb.V1;
using Grpc.Net.Client;
using Microsoft.AspNetCore.TestHost;
using SimpleBalancer;
using SimpleBalancer.Tests.App_Infrastructure.ClassFixture;
using SimpleBalancer.Tests.App_Infrastructure.Extensions;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using static Grpc.Lb.V1.LoadBalanceResponse;

namespace NetCoreGrpcIntegrationTests.AspNetCoreServerApp.Tests
{
    public sealed class InitialRequestTests : IClassFixture<CustomWebApplicationFactory<Startup>>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory<Startup> _factory;
        private readonly Fixture _fixture;
        private readonly GrpcChannel _channel;

        public InitialRequestTests(CustomWebApplicationFactory<Startup> factory)
        {
            _factory = factory;
            _client = _factory.WithWebHostBuilder(builder =>
            {
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
        public async Task ForNoRequests_UsingBalanceLoad_EnsureGracefulClose()
        {
            // Arrange
            var balancerClient = new LoadBalancer.LoadBalancerClient(_channel);
            
            // Act
            using var balancingStreaming = balancerClient.BalanceLoad();
            await balancingStreaming.RequestStream.CompleteAsync();
            var hasNextElement = await balancingStreaming.ResponseStream.MoveNext();
            balancingStreaming.Dispose();

            // Assert
            Assert.False(hasNextElement);
            Assert.Equal(StatusCode.OK, balancingStreaming.GetStatus().StatusCode);
            Assert.Equal(TaskStatus.RanToCompletion, balancingStreaming.ResponseHeadersAsync.Status);
        }

        [Trait("Category", "Integration")]
        [Fact]
        public async Task ForNoneRequest_UsingBalanceLoad_EnsureNoResponse()
        {
            // Arrange
            var balancerClient = new LoadBalancer.LoadBalancerClient(_channel);
            var request = new LoadBalanceRequest();

            // Act
            using var balancingStreaming = balancerClient.BalanceLoad();
            await balancingStreaming.RequestStream.WriteAsync(request);
            await balancingStreaming.RequestStream.CompleteAsync();
            var hasNextElement = await balancingStreaming.ResponseStream.MoveNext();

            // Assert
            Assert.Equal(LoadBalanceRequest.LoadBalanceRequestTypeOneofCase.None, request.LoadBalanceRequestTypeCase);
            Assert.False(hasNextElement);
        }

        [Trait("Category", "Integration")]
        [Fact]
        public async Task ForInitialRequest_UsingBalanceLoad_EnsureResponseMessages()
        {
            // Arrange
            var balancerClient = new LoadBalancer.LoadBalancerClient(_channel);
            var initialRequest = new InitialLoadBalanceRequest() { Name = "test-service-name" };
            var request = new LoadBalanceRequest() { InitialRequest = initialRequest };

            // Act
            using var balancingStreaming = balancerClient.BalanceLoad();
            await balancingStreaming.RequestStream.WriteAsync(request);
            await balancingStreaming.RequestStream.CompleteAsync();
            var hasNextElement = await balancingStreaming.ResponseStream.MoveNext();
            Assert.True(hasNextElement);
            var initialResponse = balancingStreaming.ResponseStream.Current;
            hasNextElement = await balancingStreaming.ResponseStream.MoveNext();
            Assert.True(hasNextElement);
            var secondResponse = balancingStreaming.ResponseStream.Current;

            // Assert
            Assert.Equal(LoadBalanceResponseTypeOneofCase.InitialResponse, initialResponse.LoadBalanceResponseTypeCase);
            Assert.NotNull(initialResponse.InitialResponse);
            Assert.Equal(TimeSpan.FromSeconds(10), initialResponse.InitialResponse.ClientStatsReportInterval.ToTimeSpan());
            Assert.Equal(LoadBalanceResponseTypeOneofCase.ServerList, secondResponse.LoadBalanceResponseTypeCase);
            Assert.NotNull(secondResponse.ServerList);
            Assert.Equal(3, secondResponse.ServerList.Servers.Count);
            Assert.All(secondResponse.ServerList.Servers, server =>
            {
                Assert.StartsWith("10.1.6.", new IPAddress(server.IpAddress.ToByteArray()).ToString());
                Assert.Equal(80, server.Port);
                Assert.False(string.IsNullOrWhiteSpace(server.LoadBalanceToken));
            });
        }
    }
}
