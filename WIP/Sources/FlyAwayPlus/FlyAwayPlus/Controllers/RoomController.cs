using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FlyAwayPlus.Helpers;
using FlyAwayPlus.Models;

namespace FlyAwayPlus.Controllers
{
    public class RoomController : Controller
    {
        //
        // GET: /Room/
        public ActionResult Index()
        {
            return View();
        }


        public ActionResult RoomDetail()
        {
            return View();
        }

        ////////////////
        // AJAX CALLS //
        ///////////////

        public void UpdatePlanEvent(int id, string newEventStart, string newEventEnd)
        {
            //DiaryEvent.UpdateDiaryEvent(id, newEventStart, newEventEnd);
        }

        public bool SaveEvent(string title, string newEventDate, string newEventTime, string newEventDuration)
        {
            var newPlan = new Plan
            {
                DatePlanStart = newEventDate + " " + newEventTime,
                LengthInMinute = int.Parse(newEventDuration),
                WorkItem = title
            };

            return GraphDatabaseHelpers.Instance.CreateNewPlanEvent(newPlan);
        }

        public JsonResult GetPlanEvents(DateTime start, DateTime end)
        {
            var apptListForDate = GraphDatabaseHelpers.Instance.LoadAllPlansInDateRange(start, end);
            
            if (!apptListForDate.Any())
            {
                return null;
            }

            var eventList = apptListForDate.Select(e => new
            {
                title = e.WorkItem,
                start = e.DatePlanStart,
                end = DateTime.ParseExact(e.DatePlanStart, FapConstants.DatetimePlanFormat, CultureInfo.InvariantCulture).AddMinutes(e.LengthInMinute).ToString(FapConstants.DatetimePlanFormat, CultureInfo.InvariantCulture),
                allday = false
            });

            var rows = eventList.ToArray();
            return Json(rows, JsonRequestBehavior.AllowGet);
        }
    }
}