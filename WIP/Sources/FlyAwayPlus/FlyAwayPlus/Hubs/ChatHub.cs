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

        public void GetListMessageInRoom(int roomID)
        {
            /*
            List<Message> listMessage = GraphDatabaseHelpers.Instance.GetListMessageInRoom(roomID);
            List<User> listUser = new List<User>();
            foreach(Message message in listMessage) {
                listUser.Add(GraphDatabaseHelpers.Instance.FindUser(message));
            }
             */
            Groups.Add(Context.ConnectionId, roomID.ToString());
            //Clients.Caller.receiveListMessage(JsonConvert.SerializeObject(listMessage), JsonConvert.SerializeObject(listUser));
        }

        public void SendChatMessageInRoom(string content, int roomID)
        {
            Clients.OthersInGroup(roomID.ToString()).getMessageInRoom(content);
        }

        public void SendChatMessage(string content, int userID, int friendID)
        {
            Clients.Others.getChatMessage(content, userID, friendID);
        }

        public void GetListFriend(int userID)
        {
            List<User> listFriend = GraphDatabaseHelpers.Instance.FindFriend(userID);
            List<Message> listMessage = new List<Message>();
            User currentUser = GraphDatabaseHelpers.Instance.FindUser(userID);
            Message message = null;
            string conversationID = "";
            for (int i = 0; i < listFriend.Count; i++)
            {
                if (listFriend[i] == null)
                {
                    message = null;
                }
                else
                {
                    if (userID < listFriend[i].userID)
                    {
                        conversationID = userID + "_" + listFriend[i].userID;
                    }
                    else
                    {
                        conversationID = listFriend[i].userID + "_" + userID;
                    }
                    message = GraphDatabaseHelpers.Instance.GetLatestMessage(conversationID);
                }
                listMessage.Add(message);
            }
            Clients.Caller.receiveListFriend(JsonConvert.SerializeObject(listFriend), JsonConvert.SerializeObject(listMessage));
        }
    }
}