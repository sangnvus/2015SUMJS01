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
        public void Connect(int roomId)
        {
            Groups.Add(Context.ConnectionId, roomId.ToString());
        }

        public void AddPostInRoom(string data, int roomId)
        {
            Clients.OthersInGroup(roomId.ToString()).receiveNewPost(data);
        }

        public void SendCommentInRoom(string content, int postId, int roomId)
        {
            Clients.OthersInGroup(roomId.ToString()).addNewCommentInRoom(content, postId);
        }
    }
}