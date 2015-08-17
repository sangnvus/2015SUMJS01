using FlyAwayPlus.Helpers;
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


        public ActionResult RoomDetail(int roomId = 0)
        {
            User user = UserHelpers.GetCurrentUser(Session);
            if (user == null)
            {
                return RedirectToAction("Index", "Home");
            }
            List<Post> listPost = GraphDatabaseHelpers.Instance.FindPostInRoom(roomId, 0);
            User admin = GraphDatabaseHelpers.Instance.FindAdminInRoom(roomId);
            List<User> listUserInRoom = GraphDatabaseHelpers.Instance.FindUserInRoom(roomId);
            List<User> listUserRequestJoinRoom = GraphDatabaseHelpers.Instance.FindUserRequestJoinRoom(roomId);
            List<Message> listMessage = GraphDatabaseHelpers.Instance.GetListMessageInRoom(roomId, 0);
            List<User> listUserOwnMessage = new List<User>();

            foreach (Message message in listMessage)
            {
                listUserOwnMessage.Add(GraphDatabaseHelpers.Instance.FindUser(message));
            }


            FindRelatedInformationPost(listPost);
            ViewData["admin"] = admin;
            ViewData["listUserInRoom"] = listUserInRoom;
            ViewData["listUserRequestJoinRoom"] = listUserRequestJoinRoom;
            ViewData["listMessage"] = listMessage;
            ViewData["listUserOwnMessage"] = listUserOwnMessage;
            ViewData["roomID"] = roomId;
            return View();
        }

        private void FindRelatedInformationPost(List<Post> listPost)
        {
            User user = UserHelpers.GetCurrentUser(Session);
            Dictionary<int, List<Photo>> listPhotoDict = new Dictionary<int, List<Photo>>();
            Dictionary<int, Video> listVideoDict = new Dictionary<int, Video>();
            Dictionary<int, User> listUserDict = new Dictionary<int, User>();
            Dictionary<int, int> dictLikeCount = new Dictionary<int, int>();
            Dictionary<int, int> dictDislikeCount = new Dictionary<int, int>();
            Dictionary<int, int> dictCommentCount = new Dictionary<int, int>();
            Dictionary<int, int> dictUserCommentCount = new Dictionary<int, int>();
            Dictionary<int, bool> isLikeDict = new Dictionary<int, bool>();
            Dictionary<int, bool> isDislikeDict = new Dictionary<int, bool>();
            Dictionary<int, List<Comment>> dictListComment = new Dictionary<int, List<Comment>>();
            List<Comment> listComment = new List<Comment>();
            Dictionary<int, User> dict = new Dictionary<int, User>();
            foreach (Post po in listPost)
            {
                listPhotoDict.Add(po.postID, GraphDatabaseHelpers.Instance.FindPhoto(po.postID));
                listVideoDict.Add(po.postID, GraphDatabaseHelpers.Instance.FindVideo(po.postID));
                listUserDict.Add(po.postID, GraphDatabaseHelpers.Instance.FindUserByPostInRoom(po));
                dictLikeCount.Add(po.postID, GraphDatabaseHelpers.Instance.CountLike(po.postID));
                dictDislikeCount.Add(po.postID, GraphDatabaseHelpers.Instance.CountDislike(po.postID));
                dictCommentCount.Add(po.postID, GraphDatabaseHelpers.Instance.CountComment(po.postID));
                dictUserCommentCount.Add(po.postID, GraphDatabaseHelpers.Instance.CountUserComment(po.postID));
                listComment = GraphDatabaseHelpers.Instance.FindComment(po);
                dictListComment.Add(po.postID, listComment);

                foreach (var comment in listComment)
                {
                    dict.Add(comment.commentID, GraphDatabaseHelpers.Instance.FindUser(comment));
                }

                if (user != null)
                {
                    isLikeDict.Add(po.postID, GraphDatabaseHelpers.Instance.IsLike(po.postID, user.userID));
                    isDislikeDict.Add(po.postID, GraphDatabaseHelpers.Instance.IsDislike(po.postID, user.userID));
                }
                else
                {
                    isLikeDict.Add(po.postID, false);
                    isDislikeDict.Add(po.postID, false);
                }
            }

            ViewData["listPost"] = listPost;
            ViewData["listPhotoDict"] = listPhotoDict;
            ViewData["listVideoDict"] = listVideoDict;
            ViewData["listUserDict"] = listUserDict;
            ViewData["dislikeCount"] = dictDislikeCount;
            ViewData["likeCount"] = dictLikeCount;
            ViewData["commentCount"] = dictCommentCount;
            ViewData["userCommentCount"] = dictUserCommentCount;
            ViewData["isLikeDict"] = isLikeDict;
            ViewData["isDislikeDict"] = isDislikeDict;
            ViewData["dictListComment"] = dictListComment;
            ViewData["dict"] = dict;

            if (listPost.Count < FapConstants.RecordsPerPage)
            {
                ViewData["isLoadMore"] = "false";
            }
            else
            {
                ViewData["isLoadMore"] = "true";
            }
        }

        ////////////////
        // AJAX CALLS //
        ///////////////

        public void UpdatePlanEvent(string id, string newEventStart, string newEventEnd)
        {
            GraphDatabaseHelpers.Instance
                .UpdatePlanEvent(id, DateTime.Parse(newEventStart, null, DateTimeStyles.RoundtripKind),
                                     DateTime.Parse(newEventEnd, null, DateTimeStyles.RoundtripKind));
        }

        public bool SaveEvent(string title, string newEventDate, string newEventTime, string newEventDuration)
        {
            var userId = ((User)Session["user"]).userID;

            var newPlan = new Plan
            {
                DatePlanStart = newEventDate + " " + newEventTime,
                LengthInMinute = int.Parse(newEventDuration) * 60,
                WorkItem = title
            };

            return GraphDatabaseHelpers.Instance.CreateNewPlanEvent(newPlan, (int)ViewData["roomID"], userId);
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
                id = e.PlanId,
                title = e.WorkItem,
                start = e.DatePlanStart,
                end = DateTime.ParseExact(e.DatePlanStart, FapConstants.DatetimeFormat, CultureInfo.InvariantCulture).AddMinutes(e.LengthInMinute).ToString(FapConstants.DatetimeFormat, CultureInfo.InvariantCulture),
                allday = false
            });

            var rows = eventList.ToArray();
            return Json(rows, JsonRequestBehavior.AllowGet);
        }

        public RedirectToRouteResult Add(FormCollection form)
        {
            var roomName    = Request.Form["roomname"];
            var roomDesc    = Request.Form["roomdesc"];
            var startdate   = DateTime.ParseExact(Request.Form["startdate"], FapConstants.DateFormat, CultureInfo.InvariantCulture);
            var enddate     = DateTime.ParseExact(Request.Form["enddate"], FapConstants.DateFormat, CultureInfo.InvariantCulture);
            var startPlace  = Request.Form["start_formatted_address"];
            var startLng    = Request.Form["start_lng"];
            var startLat    = Request.Form["start_lat"];
            var targetPlace = Request.Form["end_formatted_address"];
            var endLng      = Request.Form["end_lng"];
            var endLat      = Request.Form["end_lat"];
            var privacy     = Request.Form["privacy"];

            var currentUserId = ((User)Session["user"]).userID;

            var newRoom = new Room
            {
                RoomName = roomName,
                Description = roomDesc,
                StartDate = startdate.ToString(FapConstants.DatetimeFormat, CultureInfo.InvariantCulture),
                LengthInDays = (int)(enddate - startdate).TotalDays,
                StartLocation = startPlace,
                StartLatitude = startLat,
                StartLongitude = startLng,
                DestinationLocation = targetPlace,
                DestinationLatitude = endLat,
                DestinationLongitude = endLng,
                Privacy = privacy,
                DateCreated = DateTime.Now.ToString(FapConstants.DatetimeFormat),
                PhotoCoverUrl = string.Empty
            };

            GraphDatabaseHelpers.Instance.InsertRoom(newRoom, currentUserId);
            return RedirectToAction("Index", "Room");
        }
    }
}