using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using SignalrTesting.Api.Models;

namespace SignalrTesting.Tests.Utils
{
    public class ChatHubClient
    {
        private readonly HubConnection _connection;

        public ChatHubClient(HubConnection connection)
        {
            _connection = connection;
        }

        public Task Get(Input input)
        {
            return _connection.SendAsync("Get", input);
        }
		
        public Task SendMessageExample(Int32 requestId, Int32 age)
        {
            return _connection.SendAsync("SendMessageExample", requestId, age);
        }
		
        public Task SendMessage(Int32 requestId, String message)
        {
            return _connection.SendAsync("SendMessage", requestId, message);
        }
    }
}
