using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FlyAwayPlus.Models
{
    public class Post
    {
        public int postID { get; set; }
        public string content { get; set; }
        public string dateCreated { get; set; }
        public string privacy { get; set; }
    }
}