using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Moq;
using SignalrTesting.Api;
using Xunit;

namespace SignalrTesting.Tests.UnitsTests
{
    // Note: show differs unit vs integration tests   
    public class TestExampleWithMocks
    {
        private readonly Mock<IHubCallerClients<IChatHubClient>> _mockClients = new();
        private readonly Mock<IChatHubClient> _mockChatHubClient = new();
        private readonly Mock<HubCallerContext> _hubCallerContext = new();
        private readonly ChatHub _simpleHub;

        public TestExampleWithMocks()
        {
            _hubCallerContext.Setup(x => x.ConnectionId).Returns(Guid.NewGuid().ToString);

            _mockClients
                .Setup(clients => clients.Others)
                .Returns(_mockChatHubClient.Object);

            _mockClients
                .Setup(clients => clients.Caller)
                .Returns(_mockChatHubClient.Object);

            _simpleHub = new ChatHub(new Storage())
            {
                Clients = _mockClients.Object,
                Context = _hubCallerContext.Object
            };
        }

        [Fact]
        public async Task SignalR_OnConnect_ShouldReturn3Messages()
        {
            // act
            const string message = "32";
            await _simpleHub.SendMessage(1, message);

            // assert
            _mockChatHubClient.Verify(clientProxy => clientProxy.Submit(1, 1), Times.Once);
            _mockChatHubClient.Verify(clientProxy => clientProxy.ReceiveMessage(message, 1), Times.Once);
        }
    }
}