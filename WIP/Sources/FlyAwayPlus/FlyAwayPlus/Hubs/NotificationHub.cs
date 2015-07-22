using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;

namespace FlyAwayPlus.Hubs
{
    public class NotificationHub : Hub
    {
        public void LoadNotification(int userID)
        {

            Clients.Caller.receiveNotification("a");
        }
    }
}