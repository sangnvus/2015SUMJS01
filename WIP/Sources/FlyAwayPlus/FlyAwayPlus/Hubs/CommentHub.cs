using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;

namespace FlyAwayPlus.Hubs
{
    public class CommentHub : Hub
    {
        public void SendComment(string content)
        {
            Clients.Others.addNewComment(content);
        }
    }
}