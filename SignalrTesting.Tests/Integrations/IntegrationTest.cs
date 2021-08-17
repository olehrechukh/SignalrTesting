using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.SignalR.Client;
using SignalrTesting.Api;
using Xunit;

namespace SignalrTesting.Tests.Integrations
{
    public class IntegrationTest : IAsyncLifetime, IClassFixture<WebApplicationFactory<Startup>>
    {
        private readonly WebApplicationFactory<Startup> _factory;

        private HubConnection _hubConnection;
        private ChatHubClient _chatHubClient1;

        private readonly ChatHubInput _hubInput = new();

        public IntegrationTest(WebApplicationFactory<Startup> factory)
        {
            _factory = factory;
        }

        public Task InitializeAsync()
        {
            var httpClient = _factory.CreateClient();

            _hubConnection = new HubConnectionBuilder()
                .WithUrl($"{httpClient.BaseAddress}chatHub",
                    options => options.HttpMessageHandlerFactory = _ => _factory.Server.CreateHandler())
                .Build();

            _chatHubClient1 = new ChatHubClient(_hubConnection);

            _hubConnection.On<Dictionary<int, string>>("Snapshot", _hubInput.Snapshot);
            _hubConnection.On<int, int>("Submit", _hubInput.Submit);

            return _hubConnection.StartAsync();
        }

        [Fact]
        public void ShouldReceiveEmptySnapshot()
        {
            _hubInput.MessageSnapshot.Should().NotBeNull();
            _hubInput.MessageSnapshot.Should().BeEmpty();
        }

        [Fact]
        public async Task ShouldSubmitMessage()
        {
            var requestId = 10;
            var waitTask = _hubInput.WaitSubmit(requestId, TimeSpan.FromSeconds(1));
            await _chatHubClient1.SendMessage(requestId, "message");

            var task = await waitTask;

            task.Should().NotBe(0);
        }

        public Task DisposeAsync() => Task.CompletedTask;
    }
}