using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FlyAwayPlus.Models
{
    public class Place
    {
        public int placeID { get; set; }
        public double longitude { get; set; }
        public double latitude { get; set; }
        public string name { get; set; }
    }
}