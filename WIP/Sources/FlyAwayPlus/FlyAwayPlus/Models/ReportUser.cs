using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FlyAwayPlus.Models
{
    public class ReportUser
    {
        public int ReportId { get; set; }
        public int TypeReport { get; set; }
        public int UserReportId { get; set; }
        public int UserReportedId { get; set; }
    }
}