using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FlyAwayPlus.Models
{
    public class Notification
    {
        public String activity { get; set; }
        public String dateCreated { get; set; }
        public User user { get; set; }
        public Post post { get; set; }
        public Int16 lastActivityID { get; set; }
    }
}