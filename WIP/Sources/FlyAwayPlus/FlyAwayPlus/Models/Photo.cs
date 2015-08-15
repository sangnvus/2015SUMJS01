using FlyAwayPlus.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FlyAwayPlus.Models
{
    public class Photo
    {
        public int photoID { get; set; }
        public string url { get; set; }
        public string dateCreated { get; set; }

        public string toRealtime()
        {
            return DateHelpers.Instance.DisplayRealtime(this.dateCreated);
        }
    }
}