using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Core
{
    public class ChatMessage
    {
        public string User { get; set; }
        public string Message { get; set; }
    }

    public class Chatter
    {
        public static bool Connected;
        public static IEventStoreConnection Connection;
        public static UserCredentials _userCredentials;
        public static JsonSerializerSettings serializerSettings = new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() };

        public static void Init()
        {
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
            Connection.Disconnected += OnDisconnected;
            System.Console.WriteLine("Connecting...");
            Connection.ConnectAsync();

        }

        private static void OnDisconnected(object sender, ClientConnectionEventArgs e)
        {
            Connected = false;
        }

        public static void Subscribe(
            string room,
            Action<ChatMessage> onRecieved)
        {
            Connection.SubscribeToStreamAsync(
                room,
                false,
                OnRecieved(onRecieved));

        }

        public static void ConnectToPersistentSubscription(
            string chatRoom,
            string groupName,
            Action<ChatMessage> onRecieved)
        {
            var subSettings = PersistentSubscriptionSettingsBuilder.Create()
                .StartFromBeginning()
                .Build();
            Connection.CreatePersistentSubscriptionAsync(
                chatRoom,
                groupName,
                subSettings,
                _userCredentials);

            Connection.ConnectToPersistentSubscription(
                groupName,
                chatRoom,
                OnRecievedPersistent(onRecieved));
        }


        private static void OnConnected(object sender, ClientConnectionEventArgs e)
        {
            Connected = true;
            System.Console.WriteLine("Connected");
        }

        public static void SendMessage(string text, string user, string chatRoom)
        {
            var message = new ChatMessage
            {
                User = user,
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
                chatRoom,
                ExpectedVersion.Any,
                eventData);
        }
        public static Action<EventStoreSubscription, ResolvedEvent> OnRecieved(Action<ChatMessage> onRecieved)
        {

            return (sender, e) =>
            {
                var json = Encoding.UTF8.GetString(e.Event.Data);
                var message = JsonConvert.DeserializeObject<ChatMessage>(json);

                onRecieved(message);
            };
        }
        public static Action<EventStorePersistentSubscription, ResolvedEvent> OnRecievedPersistent(Action<ChatMessage> onRecieved)
        {
            return (sender, e) =>
            {
                var json = Encoding.UTF8.GetString(e.Event.Data);
                var message = JsonConvert.DeserializeObject<ChatMessage>(json);

                onRecieved(message);
            };
        }
    }
}
