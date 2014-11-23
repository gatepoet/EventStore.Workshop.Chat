using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Core;
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
            Chatter.User = args[0];
            Chatter.ChatRoom = args[1];
            Chatter.GroupName = args[2];
            Chatter.Main();
            while (Running)
            {
                if (Chatter.Connected)
                {
                    var message = System.Console.ReadLine();
                    if (message == "!q")
                        Running = false;
                    else
                        Chatter.SendMessage(message, Chatter.User);
                }
                else
                {
                    Thread.Sleep(100);
                }
            }
        }

    }
}
