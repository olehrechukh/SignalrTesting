using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;

namespace SignalrTesting.Blazor.Pages
{
    public partial class Index : IAsyncDisposable
    {
        protected override async Task OnInitializedAsync()
        {
            _hubConnection = new HubConnectionBuilder()
                .WithUrl(ServerUrl)
                .Build();

            _hubConnection.On<string, int>("ReceiveMessage", (message, id) =>
            {
                _messages.Add(new MessageModel(id, message, true, MessageState.Submitted));
                Console.WriteLine(JsonSerializer.Serialize(_messages));
                StateHasChanged();
            });

            _hubConnection.On<int, int>("Submit", (requestId, messageId) =>
            {
                if (!_notSubmittedMessages.TryRemove(requestId, out var messageModel))
                {
                    return;
                }

                messageModel.Submitted = MessageState.Submitted;
                messageModel.Id = messageId;
                StateHasChanged();
            });

            _hubConnection.On<Dictionary<int, string>>("Snapshot", snapshot =>
            {
                _messages = snapshot.Select(message =>
                        new MessageModel(message.Key, message.Value, true, MessageState.Submitted))
                    .ToList();
                
                StateHasChanged();
            });

            _hubConnection.On<int, string>("Error", (requestId, _) =>
            {
                if (!_notSubmittedMessages.TryRemove(requestId, out var messageModel))
                {
                    return;
                }

                messageModel.Submitted = MessageState.Error;
                StateHasChanged();
            });


            await _hubConnection.StartAsync();
        }

        private class MessageModel
        {
            public MessageModel(int? id, string text, bool input, MessageState submitted)
            {
                Id = id;
                Text = text;
                Input = input;
                Submitted = submitted;
            }

            public MessageState Submitted { get; set; }
            public int? Id { get; set; }
            public string Text { get; }
            public bool Input { get; }
            public string Side => Input ? "left" : "right";
            public string SubmittedStatus => Submitted.ToString().First().ToString();
        };

        private enum MessageState
        {
            Unknown,
            NotSubmitted,
            Submitted,
            Error
        }

        public ValueTask DisposeAsync() => _hubConnection.DisposeAsync();
    }
}