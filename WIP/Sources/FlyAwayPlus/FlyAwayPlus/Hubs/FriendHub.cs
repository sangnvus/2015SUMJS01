using System.Collections.Generic;
using Microsoft.AspNet.SignalR;
using FlyAwayPlus.Models;
using FlyAwayPlus.Helpers;
using Newtonsoft.Json;

namespace FlyAwayPlus.Hubs
{
    public class FriendHub : Hub
    {
        public void GetListFriendRequest(int userId)
        {
            List<User> listFriend = GraphDatabaseHelpers.Instance.GetListFriend(userId);
            Clients.Caller.receiveListFriendRequest(JsonConvert.SerializeObject(listFriend));
        }

        public void SendFriendRequest(int fromUserId, int toUserId)
        {
            User user = GraphDatabaseHelpers.Instance.FindUser(fromUserId);
            Clients.Others.receiveFriendRequest(JsonConvert.SerializeObject(user));
        }
    }
}