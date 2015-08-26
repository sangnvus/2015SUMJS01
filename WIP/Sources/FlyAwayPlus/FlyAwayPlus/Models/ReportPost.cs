using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FlyAwayPlus.Models
{
    public class ReportPost
    {
        public int ReportId { get; set; }
        public int PostId { get; set; }
        public int TypeRepost { get; set; }
        public int UserReportId { get; set; }
        public int UserReportedId { get; set; }
    }
}