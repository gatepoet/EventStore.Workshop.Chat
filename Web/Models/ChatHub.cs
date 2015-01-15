using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Web;
using Core;
using EventStore.ClientAPI;
using Microsoft.AspNet.SignalR;
using SignalR.Reactive;

namespace Web.Models
{
    public class ChatHub : Hub
    {
        private static Dictionary<string, Subject<ChatMessage>> subjects;
        public void Join(string room, string user)
        {
            if (subjects.ContainsKey(room))
                return;

            var subject = new Subject<ChatMessage>();
            subjects.Add(room, subject);

            subject.ToClientside().Observable<ChatHub>("onMessage");

            Chatter.Subscribe(
                room,
                OnRecieved(room));

        }

        private Action<ChatMessage> OnRecieved(string room)
        {
            return message => subjects[room].OnNext(message);
        }
    }
}