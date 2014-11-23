using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Core;
using EventStore.ClientAPI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace EventStore.Chat.Console
{
    class Program
    {
        private static bool Running = true;
        static void Main(string[] args)
        {
            var user = args[0];
            var chatRoom = args[1];
            Chatter.Init();
            Chatter.ConnectToPersistentSubscription(
                chatRoom,
                args[2],
                OnRecieved);
            while (Running)
            {
                if (Chatter.Connected)
                {
                    var message = System.Console.ReadLine();
                    if (message == "!q")
                        Running = false;
                    else
                        Chatter.SendMessage(message, user, chatRoom);
                }
                else
                {
                    Thread.Sleep(100);
                }
            }
        }

        private static void OnRecieved(ChatMessage message)
        {
            var text = string.Format(
                "{0} says:\n{1}",
                message.User,
                message.Message);

            System.Console.WriteLine(text);
        }
    }
}
