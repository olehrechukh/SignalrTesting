using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using SignalrTesting.Api.Models;

namespace SignalrTesting.Api
{
    public class ChatHub : Hub<IChatHubClient>
    {
        private readonly IStorage _storage;

        public ChatHub(IStorage storage)
        {
            _storage = storage;
        }

        public override Task OnConnectedAsync() => Clients.Caller.Snapshot(_storage.All());

        public Task Get(Input input) => Task.CompletedTask;

        public Task SendMessageExample(int requestId, int age) => Task.CompletedTask;

        public async Task SendMessage(int requestId, string message)
        {
            try
            {
                var id = _storage.Add(message);

                await Clients.Others.ReceiveMessage(message, id);
                await Clients.Caller.Submit(requestId, id);
            }
            catch (Exception e)
            {
                await Clients.Caller.Error(requestId, e.Message);
            }
        }

        public ChannelReader<int> ChannelReaderCounter(
            int count,
            int delay,
            CancellationToken cancellationToken)
        {
            var channel = Channel.CreateUnbounded<int>();

            // We don't want to await WriteItemsAsync, otherwise we'd end up waiting 
            // for all the items to be written before returning the channel back to
            // the client.

            return channel.Reader;
        }

        public async IAsyncEnumerable<int> AsyncEnumerableCounter(
            int count,
            int delay,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            for (var i = 0; i < count; i++)
            {
                // Check the cancellation token regularly so that the server will stop
                // producing items if the client disconnects.
                cancellationToken.ThrowIfCancellationRequested();

                yield return i;

                // Use the cancellationToken in other APIs that accept cancellation
                // tokens so the cancellation can flow down to them.
                await Task.Delay(delay, cancellationToken);
            }
        }
    }
}