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
    public class ChatHub : Hub
    {
        public void SendChatMessage(string content, int userID, int friendID)
        {
            Clients.Others.getChatMessage(content, userID, friendID);
        }

        public void GetListFriend(int userID)
        {
            List<User> listFriend = GraphDatabaseHelpers.FindFriend(userID);
            List<Message> listMessage = new List<Message>();
            User currentUser = GraphDatabaseHelpers.FindUser(userID);
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
                    message = GraphDatabaseHelpers.GetLatestMessage(conversationID);
                }
                listMessage.Add(message);
            }
            Clients.Caller.receiveListFriend(JsonConvert.SerializeObject(listFriend), JsonConvert.SerializeObject(listMessage));
        }
    }
}