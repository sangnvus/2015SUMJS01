using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using FlyAwayPlus.Models;
using FlyAwayPlus.Helpers;
using Newtonsoft.Json;

namespace FlyAwayPlus.Hubs
{
    public class FriendHub : Hub
    {
        public void GetListFriendRequest(int userID)
        {
            List<User> listFriend = GraphDatabaseHelpers.Instance.GetListFriend(userID);
            Clients.Caller.receiveListFriendRequest(JsonConvert.SerializeObject(listFriend));
        }
    }
}