using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FlyAwayPlus.Models
{
    public class ReportPost
    {
        public int reportID { get; set; }
        public int postID { get; set; }
        public string content { get; set; }
        public int userReportID { get; set; }
        public int userReportedID { get; set; }
    }
}