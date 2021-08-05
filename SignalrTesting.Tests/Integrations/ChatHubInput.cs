using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using SignalrTesting.Api;
using SignalrTesting.Tests.Utils;

namespace SignalrTesting.Tests.Integrations
{
    internal class ChatHubInput : IChatHubClient
    {
        public IReadOnlyDictionary<int, string> MessageSnapshot;

        private readonly ConcurrentDictionary<int, TaskCompletionSource<int>> _pendingTasks = new();

        public Task Snapshot(IReadOnlyDictionary<int, string> messages)
        {
            MessageSnapshot = messages;

            return Task.CompletedTask;
        }

        public Task ReceiveMessage(string message, int id)
        {
            throw new System.NotImplementedException();
        }

        public Task Submit(int requestId, int id)
        {
            if (_pendingTasks.TryGetValue(requestId, out var tsc))
            {
                tsc.TrySetResult(id);
            }

            return Task.CompletedTask;
        }

        public Task Error(int requestId, string message)
        {
            throw new System.NotImplementedException();
        }

        public Task<int> WaitSubmit(int id, TimeSpan timeout)
        {
            var tsc = GetOrAddNewTsc(id);
            return tsc.Task.ThrowOnTimeout(timeout);
        }

        private TaskCompletionSource<int> GetOrAddNewTsc(in int requestId) =>
            _pendingTasks.GetOrAdd(requestId,
                _ => new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously));
    }
}