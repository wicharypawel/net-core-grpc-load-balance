using AutoFixture;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Lb.V1;
using Grpc.Net.Client;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Options;
using SimpleBalancer;
using SimpleBalancer.App_Infrastructure.Options;
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
    public sealed class FallbackTests : IClassFixture<CustomWebApplicationFactory<Startup>>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory<Startup> _factory;
        private readonly Fixture _fixture;
        private readonly GrpcChannel _channel;

        public FallbackTests(CustomWebApplicationFactory<Startup> factory)
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
        public async Task ForInitialRequest_UsingBalanceLoad_EnsureResponseMessages()
        {
            // Arrange
            var balancerClient = new LoadBalancer.LoadBalancerClient(_channel);
            var firstRequest = new LoadBalanceRequest() { ClientStats = GetInitialClientStats() };

            // balancer start with normal mode

            // Act
            // Assert
            using var balancingStreaming = balancerClient.BalanceLoad();
            await balancingStreaming.RequestStream.WriteAsync(firstRequest);
            var hasNextElement = await balancingStreaming.ResponseStream.MoveNext();
            Assert.True(hasNextElement);
            var firstResponse = balancingStreaming.ResponseStream.Current;
            Assert.Equal(LoadBalanceResponseTypeOneofCase.ServerList, firstResponse.LoadBalanceResponseTypeCase);
            
            await SwitchBalancerFallbackMode(_client); // switch to fallback mode
            
            var secondRequest = new LoadBalanceRequest() { ClientStats = GetSuccessClientStats() };
            await balancingStreaming.RequestStream.WriteAsync(secondRequest);
            hasNextElement = await balancingStreaming.ResponseStream.MoveNext();
            Assert.True(hasNextElement);
            var secondResponse = balancingStreaming.ResponseStream.Current;
            Assert.Equal(LoadBalanceResponseTypeOneofCase.FallbackResponse, secondResponse.LoadBalanceResponseTypeCase);

            await SwitchBalancerFallbackMode(_client); // switch to normal mode

            var thirdRequest = new LoadBalanceRequest() { ClientStats = GetSuccessClientStats() };
            await balancingStreaming.RequestStream.WriteAsync(thirdRequest);
            hasNextElement = await balancingStreaming.ResponseStream.MoveNext();
            Assert.True(hasNextElement);
            var thirdResponse = balancingStreaming.ResponseStream.Current;
            Assert.Equal(LoadBalanceResponseTypeOneofCase.ServerList, thirdResponse.LoadBalanceResponseTypeCase);
            
            await balancingStreaming.RequestStream.CompleteAsync();
        }

        private static async Task SwitchBalancerFallbackMode(HttpClient client)
        {
            await client.GetAsync("/configure/fallback");
        }

        private static ClientStats GetInitialClientStats(DateTime? timestamp = null)
        {
            return new ClientStats()
            {
                NumCallsFinished = 0,
                NumCallsFinishedKnownReceived = 0,
                NumCallsFinishedWithClientFailedToSend = 0,
                NumCallsStarted = 0,
                Timestamp = Timestamp.FromDateTime(timestamp ?? DateTime.UtcNow),
                CallsFinishedWithDrop = { }
            };
        }

        private static ClientStats GetSuccessClientStats(DateTime? timestamp = null)
        {
            return new ClientStats()
            {
                NumCallsFinished = 120,
                NumCallsFinishedKnownReceived = 120,
                NumCallsFinishedWithClientFailedToSend = 0,
                NumCallsStarted = 120,
                Timestamp = Timestamp.FromDateTime(timestamp ?? DateTime.UtcNow),
                CallsFinishedWithDrop = { }
            };
        }
    }
}
