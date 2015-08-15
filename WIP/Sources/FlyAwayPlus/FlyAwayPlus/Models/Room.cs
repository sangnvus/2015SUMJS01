using FlyAwayPlus.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FlyAwayPlus.Models
{
    public class Room
    {
        public int roomID { get; set; }
        public string roomName { get; set; }
        public string dateCreated { get; set; }

        public string toRealtime()
        {
            return DateHelpers.Instance.DisplayRealtime(this.dateCreated);
        }
    }
}