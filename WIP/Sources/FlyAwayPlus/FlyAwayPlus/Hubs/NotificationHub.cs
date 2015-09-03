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

        public void SendNotification(string activity, int userId, int postId)
        {
            int otherUserId = GraphDatabaseHelpers.Instance.SearchUser(postId).UserId;
            if (userId == otherUserId)
            {
                return;
            }
            var notification = GraphDatabaseHelpers.Instance.FindNotificationByUser(activity, userId, postId);
            notification.User = GraphDatabaseHelpers.Instance.FindUser(userId);
            notification.Post = GraphDatabaseHelpers.Instance.FindPostById(postId);
            if (notification.DateCreated == null || notification.DateCreated.Equals(String.Empty))
            {
                notification.DateCreated = DateTime.Now.ToString(FapConstants.DatetimeFormat);
            }
            Clients.Others.receiveNewNotification(JsonConvert.SerializeObject(notification), otherUserId);
        }
    }
}