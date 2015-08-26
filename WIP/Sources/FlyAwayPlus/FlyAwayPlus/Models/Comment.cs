using FlyAwayPlus.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FlyAwayPlus.Models
{
    public class Comment
    {
        public int CommentId { get; set; }
        public string Content { get; set; }
        public string DateCreated { get; set; }

        public string ToRealtime()
        {
            return DateHelpers.Instance.DisplayRealtime(DateCreated);
        }
    }
}