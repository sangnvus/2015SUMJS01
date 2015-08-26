using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using FlyAwayPlus.Models;
using FlyAwayPlus.Helpers;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace FlyAwayPlus.Hubs
{
    public class ChatHub : Hub
    {
        public override Task OnDisconnected(bool stopCalled)
        {
            return base.OnDisconnected(stopCalled);
        }

        public void GetListMessageInRoom(int roomId)
        {
            /*
            List<Message> listMessage = GraphDatabaseHelpers.Instance.GetListMessageInRoom(roomID);
            List<User> listUser = new List<User>();
            foreach(Message message in listMessage) {
                listUser.Add(GraphDatabaseHelpers.Instance.FindUser(message));
            }
             */
            Groups.Add(Context.ConnectionId, roomId.ToString());
            //Clients.Caller.receiveListMessage(JsonConvert.SerializeObject(listMessage), JsonConvert.SerializeObject(listUser));
        }

        public void SendChatMessageInRoom(string content, int roomId)
        {
            Clients.OthersInGroup(roomId.ToString()).getMessageInRoom(content);
        }

        public void SendChatMessage(string content, int userId, int friendId)
        {
            Clients.Others.getChatMessage(content, userId, friendId);
        }

        public void GetListFriend(int userId)
        {
            List<User> listFriend = GraphDatabaseHelpers.Instance.FindFriend(userId);
            List<Message> listMessage = new List<Message>();
            User currentUser = GraphDatabaseHelpers.Instance.FindUser(userId);
            Message message = null;
            int conversationId = 0;
            for (int i = 0; i < listFriend.Count; i++)
            {
                if (listFriend[i] == null)
                {
                    message = null;
                }
                else
                {
                    conversationId = GraphDatabaseHelpers.Instance.GetConversationId(userId, listFriend[i].UserId);
                    message = GraphDatabaseHelpers.Instance.GetLatestMessage(conversationId);
                }
                listMessage.Add(message);
            }
            Clients.Caller.receiveListFriend(JsonConvert.SerializeObject(listFriend), JsonConvert.SerializeObject(listMessage));
        }
    }
}