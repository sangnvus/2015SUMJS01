using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FlyAwayPlus.Models
{
    public class Message
    {
        public int MessageId { get; set; }
        public string Content { get; set; }
        public string DateCreated { get; set; }
    }
}