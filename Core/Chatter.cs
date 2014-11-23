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
        public static string User;
        public static string GroupName;
        public static string ChatRoom;
        public static IEventStoreConnection Connection;
        public static UserCredentials _userCredentials;
        public static JsonSerializerSettings serializerSettings = new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() };

        public static void Main()
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
            Connection.ConnectAsync();
            var subSettings = PersistentSubscriptionSettingsBuilder.Create()
                .StartFromBeginning()
                .Build();
            Connection.CreatePersistentSubscriptionAsync(
                ChatRoom,
                GroupName,
                subSettings,
                _userCredentials);

            Connection.ConnectToPersistentSubscription(
                GroupName,
                ChatRoom,
                ChatMessageRecieved);

            System.Console.WriteLine("Connecting...");
        }


        private static void OnConnected(object sender, ClientConnectionEventArgs e)
        {
            Connected = true;
            System.Console.WriteLine("Connected to " + ChatRoom);
        }

        public static void SendMessage(string text, string user)
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
                ChatRoom,
                ExpectedVersion.Any,
                eventData);
        }
        private static Dictionary<string, bool> xxx = new Dictionary<string, bool>();
        private static void ChatMessageRecieved(EventStorePersistentSubscription sender, ResolvedEvent e)
        {
            var json = Encoding.UTF8.GetString(e.Event.Data);
            var message = JsonConvert.DeserializeObject<ChatMessage>(json);

            //if (message.User == User)
            //    return;
            
            var text = string.Format(
                "{0} says:\n{1}",
                message.User,
                message.Message);

            System.Console.WriteLine(text);
            //if (!xxx.ContainsKey(message.User))
            //{
            //    xxx.Add(message.User, false);
            //}
            //if (!xxx[message.User])
            //{
            //    xxx[message.User] = true;
            //    SendMessage(message.Message, message.User);
            //        Thread.Sleep(5000);
            //        xxx[message.User] = false;
            //}
        }

        class ChatMessage
        {
            public string User { get; set; }
            public string Message { get; set; }
        }

    }
}
