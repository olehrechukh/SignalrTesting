using System.IO;
using System.Reflection;
using SignalrTesting.Api;
using SignalrTesting.Tests.Utils;
using Xunit;

namespace SignalrTesting.Tests
{
    public class GeneratingTests
    {
        [Fact]
        public void GetMethods()
        {
            var render = SignalrUtils.GenerateClient<ChatHub>();
            var path = GetExecutingPath();
            
            File.WriteAllText(Path.Combine(path, "Utils", $"{nameof(ChatHub)}Client.cs"), render);
        }

        private static string GetExecutingPath() =>
            Path.Combine(Assembly.GetExecutingAssembly().Location, "..", "..", "..", "..");
    }
}