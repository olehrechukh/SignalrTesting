using System.Collections.Generic;
using System.Threading.Tasks;

namespace SignalrTesting.Api
{
    public interface IChatHubClient
    {
        Task Snapshot(IReadOnlyDictionary<int, string> messages);
        Task ReceiveMessage(string message, int id);
        Task Submit(int requestId, int id);
        Task Error(int requestId, string message);
    }
}