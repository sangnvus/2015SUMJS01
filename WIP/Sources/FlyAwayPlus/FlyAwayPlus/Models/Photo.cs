using FlyAwayPlus.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FlyAwayPlus.Models
{
    public class Photo
    {
        public int PhotoId { get; set; }
        public string Url { get; set; }
        public string DateCreated { get; set; }

        public string ToRealtime()
        {
            return DateHelpers.Instance.DisplayRealtime(this.DateCreated);
        }
    }
}