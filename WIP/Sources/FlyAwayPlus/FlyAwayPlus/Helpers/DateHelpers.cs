using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FlyAwayPlus.Models;

namespace FlyAwayPlus.Helpers
{
    public class DateHelpers
    {
        private static String[] LIST_MONTH = {   "Jan", "Feb", "March", "April", 
                                            "May", "June", "July", "Aug", 
                                            "Sep", "Oct", "Nov", "Dec"
                                        };

        public static List<String> toTimeLineDate(List<Post> listPost, Dictionary<String,List<Post>> listPostDict)
        {
            List<String> listDate = new List<string>();
            DateTime date = new DateTime();
            int year, month;
            string timeline = "";

            foreach (var post in listPost)
            {
                date = DateTime.Parse(post.dateCreated);
                year = date.Year;
                month = date.Month;

                timeline = LIST_MONTH[month - 1] + " " + year;

                List<Post> listPostTimeline = null;

                if (!listPostDict.ContainsKey(timeline))
                {
                    listPostTimeline = new List<Post>();
                    listPostTimeline.Add(post);

                    listDate.Add(timeline);
                    listPostDict.Add(timeline, listPostTimeline);
                }
                else
                {
                    listPostTimeline = listPostDict[timeline];
                    listPostTimeline.Add(post);

                    listPostDict[timeline] = listPostTimeline;
                }
            }

            return listDate.ToList();
        }

        public static string displayRealtime(string time)
        {
            string result = "";
            DateTime date = DateTime.Parse(time);
            DateTime now = DateTime.Now;
            int diffMinutes = now.Minute - date.Minute;
            int diffHours = now.Hour - date.Hour;
            if (date.AddMinutes(1).CompareTo(now) > 0)
            {
                result = String.Format("{0} seconds ago", now.Second - date.Second);
            }
            else if (date.AddHours(1).CompareTo(now) > 0)
            {
                result = String.Format("{0} minutes ago", diffMinutes < 0 ? (diffMinutes + 60) : diffMinutes);
            }
            else if (date.AddDays(1).CompareTo(now) > 0)
            {
                result = String.Format("{0} hours ago", diffHours < 0 ? (diffHours + 24) : diffHours);
            }
            else if (date.AddDays(1).Year == now.Year && date.AddDays(1).Month == now.Month && date.AddDays(1).Day == now.Day)
            {
                result = date.ToString("HH:mm") + " yesterday";
                //result = String.Format("{0:HH}:{1:mm} yesterday", date.Hour, date.Minute);
            }
            else if (date.AddYears(1).CompareTo(now) > 0)
            {
                // 14 May 12:12
                result = date.ToString("dd MMM HH:mm");
                //result = String.Format("{0:dd} {1:MMM} {2:HH}:{3:mm}", date.Day, date.Month, date.Hour, date.Minute);
            }
            else
            {
                result = date.ToString("dd MMM yyyy HH:mm");
            }
            return result;
        }
    }
}