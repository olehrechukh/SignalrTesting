using System.Collections.Generic;
using System.Threading;

namespace SignalrTesting.Api
{
    public interface IStorage
    {
        IReadOnlyDictionary<int, string> All();
        int Add(string message);
    }

    public class Storage : IStorage
    {
        private readonly Dictionary<int, string> _messages = new();

        private int _id;
        private int GetNextMessageId() => Interlocked.Increment(ref _id);

        public IReadOnlyDictionary<int, string> All() => _messages;

        public int Add(string message)
        {
            var id = GetNextMessageId();
            _messages[id] = message;
            
            return id;
        }
    }
}