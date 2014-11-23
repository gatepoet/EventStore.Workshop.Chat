using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace EventStore.Chat.Console
{
    class Program
    {
        private static bool Running = true;
        private static bool Connected;
        private static string User;
        private static string ChatRoom;
        private static IEventStoreConnection Connection;
        private static UserCredentials _userCredentials;
        private static JsonSerializerSettings serializerSettings = new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() };

        static void Main(string[] args)
        {
            User = args[0];
            ChatRoom = args[1];
            _userCredentials = new UserCredentials("admin", "changeit");
            var settings = ConnectionSettings.Create()
                .UseConsoleLogger()
                .SetDefaultUserCredentials(_userCredentials)
                .KeepReconnecting()
                .KeepRetrying();

            Connection = EventStoreConnection.Create(
                settings,
                new IPEndPoint(IPAddress.Parse("54.77.248.243"), 1113));

            Connection.Connected += OnConnected;
            Connection.ConnectAsync();
            Connection.SubscribeToStreamAsync(
                ChatRoom,
                false,
                ChatMessageRecieved);


            System.Console.WriteLine("Connecting...");
            while (Running)
            {
                if (Connected)
                {
                    var message = System.Console.ReadLine();
                    if (message == "!q")
                        Running = false;
                    else
                        SendMessage(message);
                }
                else
                {
                    Thread.Sleep(100);
                }
            }
        }

        private static void OnConnected(object sender, ClientConnectionEventArgs e)
        {
            Connected = true;
            System.Console.WriteLine("Connected to " + ChatRoom);
        }

        public static void SendMessage(string text)
        {
            var message = new ChatMessage
            {
                User = User,
                Message = text
            };

            var json = JsonConvert.SerializeObject(message, serializerSettings);
            var bytes = Encoding.UTF8.GetBytes(
                json);
            var eventData = new EventData(
                Guid.NewGuid(),
                "chatMessage",
                true,
                bytes,
                new byte[0]);

            Connection.AppendToStreamAsync(
                ChatRoom,
                ExpectedVersion.Any,
                eventData);
        }

        private static void ChatMessageRecieved(EventStoreSubscription sender, ResolvedEvent e)
        {
            var json = Encoding.UTF8.GetString(e.Event.Data);
            var message = JsonConvert.DeserializeObject<ChatMessage>(json);

            if (message.User == User)
                return;

            var text = string.Format(
                "{0} says:\n{1}",
                message.User,
                message.Message);

            System.Console.WriteLine(text);
        }

        class ChatMessage
        {
            public string User { get; set; }
            public string Message { get; set; }
        }
    }
}
