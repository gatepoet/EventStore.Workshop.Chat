using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using Core;
using Microsoft.Owin;
using Owin;
using SignalR.Reactive;
using Web.Models;

[assembly: OwinStartup(typeof(Web.Startup))]

namespace Web
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);

            InitHub();
        }

        private static void InitHub()
        {
            Chatter.Init();
        }
    }
}
