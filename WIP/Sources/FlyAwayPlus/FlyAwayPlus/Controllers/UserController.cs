using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using FlyAwayPlus.Models;
using FlyAwayPlus.Helpers;
namespace FlyAwayPlus.Controllers
{
    public class UserController : Controller
    {
        public const int RecordsPerPage = 10;
        //
        // GET: /User/
        public ActionResult Index(int id = 0)
        {
            User userSession = UserHelpers.GetCurrentUser(Session);
            User user;
            List<Post> listPost;
            List<User> friend;
            
            if (userSession == null)
            {
                return RedirectToAction("Index", "Home");
            }

            if (userSession.userID == id || id == 0)
            {
                user = userSession;
                listPost = GraphDatabaseHelpers.Instance.FindPostOfUser(userSession);
                friend = GraphDatabaseHelpers.Instance.FindFriend(userSession);

                FindRelatedInformationPost(listPost);
            }
            else
            {
                user = GraphDatabaseHelpers.Instance.FindUser(id);
                if (user == null)
                {
                    return RedirectToAction("Index", "Home");
                }
                listPost = GraphDatabaseHelpers.Instance.FindPostOfOtherUser(userSession, user);
                friend = GraphDatabaseHelpers.Instance.FindFriend(user);

                FindRelatedInformationPost(listPost);
            }

            ViewData["userSession"] = userSession;
            ViewData["friend"] = friend;
            ViewData["isFriend"] = GraphDatabaseHelpers.Instance.GetFriendType(userSession.userID, user.userID);
            return View(user);
        }

        private void FindRelatedInformationPost(List<Post> listPost)
        {
            User user = UserHelpers.GetCurrentUser(Session);
            Dictionary<int, List<Photo>> listPhotoDict = new Dictionary<int, List<Photo>>();
            Dictionary<int, Video> listVideoDict = new Dictionary<int, Video>();
            Dictionary<int, Place> listPlaceDict = new Dictionary<int, Place>();
            Dictionary<int, User> listUserDict = new Dictionary<int, User>();
            Dictionary<int, int> dictLikeCount = new Dictionary<int, int>();
            Dictionary<int, int> dictDislikeCount = new Dictionary<int, int>();
            Dictionary<int, int> dictCommentCount = new Dictionary<int, int>();
            Dictionary<int, int> dictUserCommentCount = new Dictionary<int, int>();
            Dictionary<int, bool> isLikeDict = new Dictionary<int, bool>();
            Dictionary<int, bool> isDislikeDict = new Dictionary<int, bool>();
            Dictionary<int, bool> isWishDict = new Dictionary<int, bool>();
            List<Photo> listPhoto = new List<Photo>();
            List<Place> listPlace = new List<Place>();

            foreach (Post po in listPost)
            {
                listPhotoDict.Add(po.postID, GraphDatabaseHelpers.Instance.FindPhoto(po.postID));
                listVideoDict.Add(po.postID, GraphDatabaseHelpers.Instance.FindVideo(po.postID));
                listPlaceDict.Add(po.postID, GraphDatabaseHelpers.Instance.FindPlace(po));
                listUserDict.Add(po.postID, GraphDatabaseHelpers.Instance.FindUser(po));
                dictLikeCount.Add(po.postID, GraphDatabaseHelpers.Instance.CountLike(po.postID));
                dictDislikeCount.Add(po.postID, GraphDatabaseHelpers.Instance.CountDislike(po.postID));
                dictCommentCount.Add(po.postID, GraphDatabaseHelpers.Instance.CountComment(po.postID));
                dictUserCommentCount.Add(po.postID, GraphDatabaseHelpers.Instance.CountUserComment(po.postID));

                if (user != null)
                {
                    isLikeDict.Add(po.postID, GraphDatabaseHelpers.Instance.IsLike(po.postID, user.userID));
                    isDislikeDict.Add(po.postID, GraphDatabaseHelpers.Instance.IsDislike(po.postID, user.userID));
                    isWishDict.Add(po.postID, GraphDatabaseHelpers.Instance.IsWish(po.postID, user.userID));
                }
                else
                {
                    isLikeDict.Add(po.postID, false);
                    isDislikeDict.Add(po.postID, false);
                    isWishDict.Add(po.postID, false);
                }
            }

            ViewData["listPost"] = listPost;
            ViewData["listPhotoDict"] = listPhotoDict;
            ViewData["listVideoDict"] = listVideoDict;
            ViewData["listPlaceDict"] = listPlaceDict;
            ViewData["listUserDict"] = listUserDict;
            ViewData["dislikeCount"] = dictDislikeCount;
            ViewData["likeCount"] = dictLikeCount;
            ViewData["commentCount"] = dictCommentCount;
            ViewData["userCommentCount"] = dictUserCommentCount;
            ViewData["isLikeDict"] = isLikeDict;
            ViewData["isDislikeDict"] = isDislikeDict;
            ViewData["isWishDict"] = isWishDict;
            ViewData["typePost"] = "index";

            if (listPost.Count < RecordsPerPage)
            {
                ViewData["isLoadMore"] = "false";
            }
            else
            {
                ViewData["isLoadMore"] = "true";
            }
        }
        public ActionResult Comment(int postId, string content)
        {
            User user = UserHelpers.GetCurrentUser(Session);
            Dictionary<int, FlyAwayPlus.Models.User> dict = new Dictionary<int, Models.User>();
            Comment comment = new Comment();
            comment.content = content;
            comment.content = comment.content.Replace("\n","\\n");
            comment.dateCreated = DateTime.Now.ToString(FapConstants.DatetimeFormat);

            bool success = GraphDatabaseHelpers.Instance.InsertComment(postId, comment, user.userID);
            dict.Add(comment.commentID, user);
            
            ViewData["dict"] = dict;
            return PartialView("_PostDetailPartial", comment);
        }

        public JsonResult EditComment(int commentID, string content)
        {
            Comment comment = new Comment();
            comment.commentID = commentID;
            comment.content = content;
            comment.content = comment.content.Replace("\n", "\\n");
            comment.dateCreated = DateTime.Now.ToString(FapConstants.DatetimeFormat);

            bool success = GraphDatabaseHelpers.Instance.EditComment(comment);
            return Json(success);
        }

        public JsonResult DeleteComment(int commentID)
        {
            User user = UserHelpers.GetCurrentUser(Session);
            bool success = GraphDatabaseHelpers.Instance.DeleteComment(commentID, user.userID);
            return Json(success);
        }

        public JsonResult Like(int postId)
        {
            User user = UserHelpers.GetCurrentUser(Session);
            Boolean success = false;
            if (user != null)
            {
                int like = GraphDatabaseHelpers.Instance.FindLike(user.userID, postId);
                if (like == 0)
                {
                    // User like post and delete exist dislike
                    success = GraphDatabaseHelpers.Instance.InsertLike(user.userID, postId);
                    GraphDatabaseHelpers.Instance.DeleteDislike(user.userID, postId);
                }
                else
                {
                    // delete exist like post
                    GraphDatabaseHelpers.Instance.DeleteLike(user.userID, postId);
                }
            }
            return Json(success);
        }

        public JsonResult Dislike(int postId)
        {
            User user = UserHelpers.GetCurrentUser(Session);
            Boolean success = false;
            if (user != null)
            {
                int dislike = GraphDatabaseHelpers.Instance.FindDislike(user.userID, postId);
                if (dislike == 0)
                {
                    // user dislike post and delete exist like
                    success = GraphDatabaseHelpers.Instance.InsertDislike(user.userID, postId);
                    GraphDatabaseHelpers.Instance.DeleteLike(user.userID, postId);
                }
                else
                {
                    // delete exist dislike
                    GraphDatabaseHelpers.Instance.DeleteDislike(user.userID, postId);
                }
            }
            return Json(success);
        }

        public JsonResult AddToWishlist(int postID)
        {
            User user = UserHelpers.GetCurrentUser(Session);
            bool success = false;
            if (user != null)
            {
                success = GraphDatabaseHelpers.Instance.AddToWishList(postID, user.userID);
            }
            return Json(success);
        }

        public JsonResult AddWishPlace(int placeID, int userID)
        {
            User user = UserHelpers.GetCurrentUser(Session);
            bool success = false;
            if (user != null)
            {
                success = GraphDatabaseHelpers.Instance.AddToWishlist(placeID, userID);
            }
            return Json(success);
        }

        public JsonResult AddFriend(int userID, int otherUserID)
        {
            User user = UserHelpers.GetCurrentUser(Session);
            bool success = false;
            if (user != null)
            {
                success = GraphDatabaseHelpers.Instance.AddFriend(userID, otherUserID);
            }
            return Json(success);
        }

        public JsonResult DeclineRequestFriend(int userID, int otherUserID)
        {
            User user = UserHelpers.GetCurrentUser(Session);
            bool success = false;
            if (user != null)
            {
                success = GraphDatabaseHelpers.Instance.DeclineRequestFriend(userID, otherUserID);
            }
            return Json(success);
        }

        public JsonResult SendRequestFriend(int userID, int otherUserID)
        {
            User user = UserHelpers.GetCurrentUser(Session);
            bool success = false;
            if (user != null)
            {
                success = GraphDatabaseHelpers.Instance.SendRequestFriend(userID, otherUserID);
            }
            return Json(success);
        }

        public JsonResult Unfriend(int userID, int otherUserID)
        {
            User user = UserHelpers.GetCurrentUser(Session);
            bool success = false;
            if (user != null)
            {
                success = GraphDatabaseHelpers.Instance.Unfriend(userID, otherUserID);
            }
            return Json(success);
        }

        public JsonResult RemoveFromWishlist(int postID)
        {
            User user = UserHelpers.GetCurrentUser(Session);
            bool success = false;
            if (user != null)
            {
                success = GraphDatabaseHelpers.Instance.RemoveFromWishList(postID, user.userID);
            }
            return Json(success);
        }

        public JsonResult RemoveWishPlace(int placeID, int userID)
        {
            User user = UserHelpers.GetCurrentUser(Session);
            bool success = false;
            if (user != null)
            {
                success = GraphDatabaseHelpers.Instance.RemoveFromWishlist(placeID, userID);
            }
            return Json(success);
        }

        public JsonResult GetMessage(int friendID)
        {
            User user = UserHelpers.GetCurrentUser(Session);
            List<Message> listMessage = new List<Message>();
            List<User> listUser = new List<User>();
            if (user != null)
            {
                listMessage = GraphDatabaseHelpers.Instance.GetListMessage(user.userID, friendID, 10);
                for (int i = 0; i < listMessage.Count; i++)
                {
                    listUser.Add(GraphDatabaseHelpers.Instance.FindUser(listMessage[i]));
                }
            }
            KeyValuePair<List<Message>, List<User>> returnObject = new KeyValuePair<List<Message>,List<User>>(listMessage, listUser);
            return Json(returnObject);
        }

        public JsonResult CreateMessage(int friendID, string content)
        {
            User user = UserHelpers.GetCurrentUser(Session);
            Message message = null;
            if (user != null)
            {
                message = GraphDatabaseHelpers.Instance.CreateMessage(content, user.userID, friendID);

            }
            return Json(message);
        }

        public JsonResult CreateMessageInRoom(int roomID, string content)
        {
            User user = UserHelpers.GetCurrentUser(Session);
            Message message = null;
            if (user != null)
            {
                message = GraphDatabaseHelpers.Instance.CreateMessageInRoom(roomID, user.userID, content);

            }
            return Json(message);
        }

        public JsonResult EditProfile(string firstName, string lastName, string address, string gender, string phoneNumber,
                            string dateOfBirth, string password)
        {
            User user = UserHelpers.GetCurrentUser(Session);
            bool success = false;
            if (user != null)
            {
                user.firstName = firstName;
                user.lastName = lastName;
                user.address = address;
                user.gender = gender;
                user.phoneNumber = phoneNumber;
                user.dateOfBirth = dateOfBirth;
                user.password = password;

                success = GraphDatabaseHelpers.Instance.EditProfile(user);
            }
            return Json(success);
        }
        public JsonResult ReportPost(int postID, int userReportID, int userReportedID, int typeReport)
        {
            bool success = false;

            ReportPost reportPost = new ReportPost();
            reportPost.postID = postID;
            reportPost.typeRepost = typeReport;
            reportPost.userReportID = userReportID;
            reportPost.userReportedID = userReportedID;

            GraphDatabaseHelpers.Instance.InsertReportPost(reportPost);

            //Send warning email to reported user
            var email = GraphDatabaseHelpers.Instance.GetEmailByUserId(userReportedID);
            string senderID = "flyawayplus.system@gmail.com"; // use sender’s email id here..
            const string senderPassword = "doan2015"; // sender password here…
            try
            {
                var smtp = new SmtpClient
                {
                    Host = "smtp.gmail.com", // smtp server address here…
                    Port = 587,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    Credentials = new NetworkCredential(senderID, senderPassword),
                    Timeout = 30000,
                };
                var message = new MailMessage(senderID, email, "Report post",
                    "Your post is reported");
                smtp.Send(message);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

            }

            return Json(success);
        }

        public JsonResult ReportUser(int userReportID, int userReportedID, int typeReport)
        {
            bool success = false;

            ReportUser reportUser = new ReportUser();
            reportUser.typeReport = typeReport;
            reportUser.userReportID = userReportID;
            reportUser.userReportedID = userReportedID;

            GraphDatabaseHelpers.Instance.InsertReportUser(reportUser);

            return Json(success);
        }
	}
}