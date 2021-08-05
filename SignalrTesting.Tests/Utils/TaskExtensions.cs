using System;
using System.Threading;
using System.Threading.Tasks;

namespace SignalrTesting.Tests.Utils
{
    public static class TaskExtensions
    {
        public static async Task<T> ThrowOnTimeout<T>(
            this Task<T> task,
            TimeSpan timeout,
            CancellationToken cancelToken = default)
        {
            if (task == null)
            {
                throw new ArgumentNullException(nameof(task));
            }

            if (task.IsCompleted || timeout == Timeout.InfiniteTimeSpan)
            {
                return await task.ConfigureAwait(false); // propagate exception/cancellation
            }

            if (timeout < Timeout.InfiniteTimeSpan)
            {
                throw new ArgumentOutOfRangeException(nameof(timeout));
            }

            if (timeout == TimeSpan.Zero)
            {
                throw new TimeoutException();
            }

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancelToken);
            var delayTask = Task.Delay(timeout, cts.Token);
            var firstToFinish = await Task.WhenAny(task, delayTask).ConfigureAwait(false);
            if (firstToFinish == delayTask)
            {
                throw new TimeoutException();
            }

            cts.Cancel(); // ensure that the Delay task is cleaned up
            return await task.ConfigureAwait(false); // propagate exception/cancellation
        }
    }
}