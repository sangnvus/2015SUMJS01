using System;

namespace FlyAwayPlus.Models
{
    public class Plan
    {
        public int PlanId { get; set; }
        public string WorkItem { get; set; }
        public int LengthInMinute { get; set; }
        public string DatePlanStart { get; set; }
    }
}