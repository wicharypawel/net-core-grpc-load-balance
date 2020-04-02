using AutoFixture;
using Google.Protobuf.WellKnownTypes;
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
    public sealed class ClientStatsTests : IClassFixture<CustomWebApplicationFactory<Startup>>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory<Startup> _factory;
        private readonly Fixture _fixture;
        private readonly GrpcChannel _channel;

        public ClientStatsTests(CustomWebApplicationFactory<Startup> factory)
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

            // Act
            using var balancingStreaming = balancerClient.BalanceLoad();
            await balancingStreaming.RequestStream.WriteAsync(firstRequest);
            var hasNextElement = await balancingStreaming.ResponseStream.MoveNext();
            Assert.True(hasNextElement);
            var firstResponse = balancingStreaming.ResponseStream.Current;
            var secondRequest = new LoadBalanceRequest() { ClientStats = GetSuccessClientStats() };
            await balancingStreaming.RequestStream.WriteAsync(secondRequest);
            hasNextElement = await balancingStreaming.ResponseStream.MoveNext();
            Assert.True(hasNextElement);
            var secondResponse = balancingStreaming.ResponseStream.Current;
            await balancingStreaming.RequestStream.CompleteAsync();

            // Assert
            AssertServerListForClientStats(firstResponse);
            AssertServerListForClientStats(secondResponse);
        }

        private static void AssertServerListForClientStats(LoadBalanceResponse response)
        {
            Assert.Equal(LoadBalanceResponseTypeOneofCase.ServerList, response.LoadBalanceResponseTypeCase);
            Assert.NotNull(response.ServerList);
            Assert.Equal(3, response.ServerList.Servers.Count);
            Assert.All(response.ServerList.Servers, server =>
            {
                Assert.StartsWith("10.1.6.", new IPAddress(server.IpAddress.ToByteArray()).ToString());
                Assert.Equal(80, server.Port);
                Assert.False(string.IsNullOrWhiteSpace(server.LoadBalanceToken));
            });
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
