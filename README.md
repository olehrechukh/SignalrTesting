# Strogly signalr client for testing

**Experimental API endpoint specification** and code generator for [SignalR Core](https://github.com/aspnet/SignalR).
**SourceGeneratorGen** is linked to API SignalR [source generator](https://devblogs.microsoft.com/dotnet/introducing-c-source-generators/).

Run **SignalrTesting.Api** to generated c# Client Code code.
# Demo
Hub:
```
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
}
```
Generated client:
```
public class ChatHubClient
{
    private readonly HubConnection _connection;

    public ChatHubClient (HubConnection connection)
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
```
