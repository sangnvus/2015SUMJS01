using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using System.Web.Mvc;
using FlyAwayPlus.Models;
using FlyAwayPlus.Helpers;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace FlyAwayPlus.Hubs
{
    public class RoomHub : Hub
    {
        public override Task OnDisconnected(bool stopCalled)
        {
            return base.OnDisconnected(stopCalled);
        }

        public void AddPost(FormCollection form)
        {
            string x = form["place"];
            //Clients.All.hello();
        }
    }
}