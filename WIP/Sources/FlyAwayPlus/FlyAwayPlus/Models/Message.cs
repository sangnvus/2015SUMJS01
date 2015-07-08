using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FlyAwayPlus.Models
{
    public class Message
    {
        public int messageID { get; set; }
        public string content { get; set; }
        public string dateCreated { get; set; }
    }
}