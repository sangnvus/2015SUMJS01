using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using FlyAwayPlus.Helpers;
using System.Web.Mvc;
using Newtonsoft.Json;
namespace FlyAwayPlus.Hubs
{
    public class NotificationHub : Hub
    {
        public void LoadNotification(int userId, int lastActivityId)
        {
            var listNotification = GraphDatabaseHelpers.Instance.GetNotification(userId, lastActivityId, 5);
            Clients.Caller.receiveNotification(JsonConvert.SerializeObject(listNotification));
        }
    }
}