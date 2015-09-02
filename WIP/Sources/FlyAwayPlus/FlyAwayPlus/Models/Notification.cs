using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FlyAwayPlus.Models
{
    public class Notification
    {
        public String Activity { get; set; }
        public String DateCreated { get; set; }
        public User User { get; set; }
        public Post Post { get; set; }
        public Int32 LastActivityId { get; set; }
        public int IsViewed { get; set; }
    }
}