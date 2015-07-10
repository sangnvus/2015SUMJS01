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
        private static String FORMAT = "yyyy/MM/dd hh:mm:ss";

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
    }
}