using FlyAwayPlus.Helpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FlyAwayPlus.Models;

namespace FlyAwayPlus.Controllers
{
    public class RoomController : Controller
    {
        //
        // GET: /Room/
        public ActionResult Index(int id = -1)
        {
            User user = UserHelpers.GetCurrentUser(Session);
            if (user == null)
            {
                RedirectToAction("Index", "Home");
            }
            List<Room> listRoom;
            List<User> listAdminRoom = new List<User>();

            if (id == -1)
            {
                listRoom = GraphDatabaseHelpers.Instance.SearchRoomByKeyword("");   // search All room
                foreach (var room in listRoom)
                {
                    listAdminRoom.Add(GraphDatabaseHelpers.Instance.FindAdminInRoom(room.RoomId));
                }
            }
            else
            {
                listRoom = GraphDatabaseHelpers.Instance.SearchRoomByUserId(user.UserId);   // search All room
                for (int i = 0; i < listRoom.Count; i++)
                {
                    listAdminRoom.Add(user);
                }
            }

            ViewData["listRoom"] = listRoom;
            ViewData["listAdminRoom"] = listAdminRoom;
            return View();
        }


        public ActionResult RoomDetail(string id)
        {
            int roomId;
            if (id == null || !int.TryParse(id, out roomId))
            {
                return RedirectToAction("Index", "Home");
            }

            User user = UserHelpers.GetCurrentUser(Session);
            if (user == null)
            {
                return RedirectToAction("Index", "Home");
            }

            Room roomInfo = GraphDatabaseHelpers.Instance.GetRoomInformation(roomId);
            List<Post> listPost = GraphDatabaseHelpers.Instance.FindPostInRoom(roomId, 0);
            User admin = GraphDatabaseHelpers.Instance.FindAdminInRoom(roomId);
            List<User> listUserInRoom = GraphDatabaseHelpers.Instance.FindUserInRoom(roomId);
            List<User> listUserRequestJoinRoom = GraphDatabaseHelpers.Instance.FindUserRequestJoinRoom(roomId);
            List<Message> listMessage = GraphDatabaseHelpers.Instance.GetListMessageInRoom(roomId, 0);
            List<User> listUserOwnMessage = listMessage.Select(message => GraphDatabaseHelpers.Instance.FindUser(message)).ToList();

            var roomStartDate = DateTime.ParseExact(roomInfo.StartDate, FapConstants.DateFormat, CultureInfo.InvariantCulture);
            var roomEndDate = roomStartDate.AddDays(roomInfo.LengthInDays);

            List<Plan> listGeneralPlan = GraphDatabaseHelpers.Instance.LoadAllPlansInDateRange(roomStartDate, roomEndDate, FapConstants.PlanGeneral, roomId);
            ViewData["dictPlanIdPICs"] = listGeneralPlan
                                        .Select(p => p.PlanId)
                                        .ToDictionary(pid => pid, pid => GraphDatabaseHelpers.Instance.GetPersonInCharge(pid));

            ViewData["dictPlanIdCreator"] = listGeneralPlan
                                        .Select(p => p.PlanId)
                                        .ToDictionary(pid => pid, pid => GraphDatabaseHelpers.Instance.GetPersonCreatesPlan(pid));

            FindRelatedInformationPost(listPost);
            ViewData["roomInfo"] = roomInfo;
            ViewData["admin"] = admin;
            ViewData["listUserInRoom"] = listUserInRoom;
            ViewData["listUserRequestJoinRoom"] = listUserRequestJoinRoom;
            ViewData["listMessage"] = listMessage;
            ViewData["listUserOwnMessage"] = listUserOwnMessage;
            ViewData["listGeneralPlan"] = listGeneralPlan;
            Session["roomID"] = ViewData["roomID"] = roomId;
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
            Dictionary<int, User> dict = new Dictionary<int, User>();
            foreach (int postId in listPost.Select(p => p.PostId))
            {
                listPhotoDict.Add(postId, GraphDatabaseHelpers.Instance.FindPhoto(postId));
                listVideoDict.Add(postId, GraphDatabaseHelpers.Instance.FindVideo(postId));
                listUserDict.Add(postId, GraphDatabaseHelpers.Instance.FindUserByPostInRoom(postId));
                dictLikeCount.Add(postId, GraphDatabaseHelpers.Instance.CountLike(postId));
                dictDislikeCount.Add(postId, GraphDatabaseHelpers.Instance.CountDislike(postId));
                dictCommentCount.Add(postId, GraphDatabaseHelpers.Instance.CountComment(postId));
                dictUserCommentCount.Add(postId, GraphDatabaseHelpers.Instance.CountUserComment(postId));
                var listComment = GraphDatabaseHelpers.Instance.FindComment(postId);
                dictListComment.Add(postId, listComment);

                foreach (var comment in listComment)
                {
                    dict.Add(comment.CommentId, GraphDatabaseHelpers.Instance.FindUser(comment));
                }

                if (user != null)
                {
                    isLikeDict.Add(postId, GraphDatabaseHelpers.Instance.IsLike(postId, user.UserId));
                    isDislikeDict.Add(postId, GraphDatabaseHelpers.Instance.IsDislike(postId, user.UserId));
                }
                else
                {
                    isLikeDict.Add(postId, false);
                    isDislikeDict.Add(postId, false);
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

        public void UpdateTimelinePlan(string id, string newEventStart, string newEventEnd)
        {
            GraphDatabaseHelpers.Instance
                .UpdatePlanEvent(id, DateTime.Parse(newEventStart, null, DateTimeStyles.RoundtripKind),
                                     DateTime.Parse(newEventEnd, null, DateTimeStyles.RoundtripKind));
        }

        public bool SaveTimelinePlan(string title, string newEventDate, string newEventTime, string newEventDuration)
        {
            var userId = int.Parse(@Session["UserId"] + string.Empty);

            var newPlan = new Plan
            {
                DatePlanStart = newEventDate + " " + newEventTime,
                LengthInMinute = int.Parse(newEventDuration) * 60,
                WorkItem = title,
                PlanType = FapConstants.PlanTimeline,
                DatePlanCreated = DateTime.Now.ToString(FapConstants.DatetimeFormat, CultureInfo.InvariantCulture)
            };

            return GraphDatabaseHelpers.Instance.InsertPlan(newPlan, (int)Session["roomID"], userId);
        }

        public JsonResult GetPlanEvents(DateTime start, DateTime end)
        {
            int roomid = (int)Session["roomID"];

            var apptListForDate = GraphDatabaseHelpers.Instance.LoadAllPlansInDateRange(start, end, FapConstants.PlanTimeline, roomid);

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

        public RedirectToRouteResult CreateRoom(FormCollection form)
        {
            var roomName = Request.Form["roomname"];
            var roomDesc = Request.Form["roomdesc"];
            var startdate = DateTime.ParseExact(Request.Form["startdate"], FapConstants.DateFormat, CultureInfo.InvariantCulture);
            var enddate = DateTime.ParseExact(Request.Form["enddate"], FapConstants.DateFormat, CultureInfo.InvariantCulture);
            var startPlace = Request.Form["start_formatted_address"];
            var startLng = Request.Form["start_lng"];
            var startLat = Request.Form["start_lat"];
            var targetPlace = Request.Form["end_formatted_address"];
            var endLng = Request.Form["end_lng"];
            var endLat = Request.Form["end_lat"];
            var privacy = Request.Form["privacy"];
            int maxNoOfSlots;
            int.TryParse(Request.Form["maxnoslots"], out maxNoOfSlots);

            var currentUserId = int.Parse(@Session["UserId"] + string.Empty);

            var newRoom = new Room
            {
                RoomName = roomName,
                Description = roomDesc,
                MaxNoSlots = maxNoOfSlots,
                StartDate = startdate.ToString(FapConstants.DateFormat, CultureInfo.InvariantCulture),
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

        public JsonResult AddGeneralPlan(string workportion, string note, string assignee, string startdate)
        {
            // TODO: Get list of assignees.
            assignee = "10000;10001";

            var assignerId = int.Parse(@Session["UserId"] + string.Empty);
            int roomid = (int)Session["roomID"];
            List<int> assigneesInt = assignee.Split(';').Select(int.Parse).ToList();

            var newPlan = new Plan
            {
                WorkItem = workportion,
                WorkItemDetail = note,
                DatePlanStart = startdate + " 00:00:00",
                PlanType = FapConstants.PlanGeneral,
                DatePlanCreated = DateTime.Now.ToString(FapConstants.DatetimeFormat, CultureInfo.InvariantCulture)
            };

            GraphDatabaseHelpers.Instance.InsertPlan(newPlan, roomid, assignerId, assigneesInt);

            return Json(assigneesInt.Select(a => GraphDatabaseHelpers.Instance.FindUser(a)));
        }

        public JsonResult ExportEstimationData(int roomId)
        {
            var estimationList = new List<object>
            {
                new
                {
                    id = 1,
                    payment = "Mua vé tàu cho cả nhóm",
                    price = 1200000,
                    creator = "Duc Filan",
                    payer = "Thuy Le",
                    datecreated = "2015-08-24"
                },
                new
                {
                    id = 1,
                    payment = "Tiền đặt phòng khách sạn",
                    price = 1200000,
                    creator = "Duc Filan",
                    payer = "Thuy Le",
                    datecreated = "2015-08-24"
                },
                new
                {
                    id = 1,
                    payment = "Mua lều",
                    price = 1200000,
                    creator = "Duc Filan",
                    payer = "Thuy Le",
                    datecreated = "2015-08-24"
                },
                new
                {
                    id = 1,
                    payment = "Mua đồ ăn",
                    price = 1200000,
                    creator = "Duc Filan",
                    payer = "Thuy Le",
                    datecreated = "2015-08-24"
                },
                new
                {
                    id = 1,
                    payment = "Thuê xe máy",
                    price = 1200000,
                    creator = "Duc Filan",
                    payer = "Thuy Le",
                    datecreated = "2015-08-24"
                }
            };

            return Json(estimationList, JsonRequestBehavior.AllowGet);
        }
    }
}