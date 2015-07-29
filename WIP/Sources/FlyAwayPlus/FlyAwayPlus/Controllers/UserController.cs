using System;
using System.Collections.Generic;
using System.Linq;
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
            List<Photo> listPhoto = new List<Photo>();
            List<User> friend;
            
            if (userSession == null)
            {
                return RedirectToAction("Index", "Home");
            }

            if (userSession.userID == id || id == 0)
            {
                user = userSession;
                listPost = GraphDatabaseHelpers.FindPostOfUser(userSession);
                friend = GraphDatabaseHelpers.FindFriend(userSession);

                FindRelatedInformationPost(listPost);
            }
            else
            {
                user = GraphDatabaseHelpers.FindUser(id);
                if (user == null)
                {
                    return RedirectToAction("Index", "Home");
                }
                listPost = GraphDatabaseHelpers.FindPostOfOtherUser(userSession, user);
                friend = GraphDatabaseHelpers.FindFriend(user);

                FindRelatedInformationPost(listPost);
            }

            ViewData["userSession"] = userSession;
            ViewData["friend"] = friend;
            ViewData["isFriend"] = GraphDatabaseHelpers.GetFriendType(userSession.userID, user.userID);
            return View(user);
        }

        private void FindRelatedInformationPost(List<Post> listPost)
        {
            User user = UserHelpers.GetCurrentUser(Session);
            Dictionary<int, Photo> listPhotoDict = new Dictionary<int, Photo>();
            Dictionary<int, Place> listPlaceDict = new Dictionary<int, Place>();
            Dictionary<int, User> listUserDict = new Dictionary<int, User>();
            Dictionary<int, int> dictLikeCount = new Dictionary<int, int>();
            Dictionary<int, int> dictDislikeCount = new Dictionary<int, int>();
            Dictionary<int, int> dictCommentCount = new Dictionary<int, int>();
            Dictionary<int, int> dictUserCommentCount = new Dictionary<int, int>();
            Dictionary<int, bool> isLikeDict = new Dictionary<int, bool>();
            Dictionary<int, bool> isDislikeDict = new Dictionary<int, bool>();
            Dictionary<int, bool> isWishDict = new Dictionary<int, bool>();

            foreach (Post po in listPost)
            {
                listPhotoDict.Add(po.postID, GraphDatabaseHelpers.FindPhoto(po));
                listPlaceDict.Add(po.postID, GraphDatabaseHelpers.FindPlace(po));
                listUserDict.Add(po.postID, GraphDatabaseHelpers.FindUser(po));
                dictLikeCount.Add(po.postID, GraphDatabaseHelpers.CountLike(po.postID));
                dictDislikeCount.Add(po.postID, GraphDatabaseHelpers.CountDislike(po.postID));
                dictCommentCount.Add(po.postID, GraphDatabaseHelpers.CountComment(po.postID));
                dictUserCommentCount.Add(po.postID, GraphDatabaseHelpers.CountUserComment(po.postID));

                if (user != null)
                {
                    isLikeDict.Add(po.postID, GraphDatabaseHelpers.isLike(po.postID, user.userID));
                    isDislikeDict.Add(po.postID, GraphDatabaseHelpers.isDislike(po.postID, user.userID));
                    isWishDict.Add(po.postID, GraphDatabaseHelpers.isWish(po.postID, user.userID));
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

            bool success = GraphDatabaseHelpers.InsertComment(postId, comment, user.userID);
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

            bool success = GraphDatabaseHelpers.EditComment(comment);
            return Json(success);
        }

        public JsonResult DeleteComment(int commentID)
        {
            User user = UserHelpers.GetCurrentUser(Session);
            bool success = GraphDatabaseHelpers.DeleteComment(commentID, user.userID);
            return Json(success);
        }

        public JsonResult Like(int postId)
        {
            User user = UserHelpers.GetCurrentUser(Session);
            Boolean success = false;
            if (user != null)
            {
                int like = GraphDatabaseHelpers.FindLike(user.userID, postId);
                if (like == 0)
                {
                    // User like post and delete exist dislike
                    success = GraphDatabaseHelpers.InsertLike(user.userID, postId);
                    GraphDatabaseHelpers.DeleteDislike(user.userID, postId);
                }
                else
                {
                    // delete exist like post
                    GraphDatabaseHelpers.DeleteLike(user.userID, postId);
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
                int dislike = GraphDatabaseHelpers.FindDislike(user.userID, postId);
                if (dislike == 0)
                {
                    // user dislike post and delete exist like
                    success = GraphDatabaseHelpers.InsertDislike(user.userID, postId);
                    GraphDatabaseHelpers.DeleteLike(user.userID, postId);
                }
                else
                {
                    // delete exist dislike
                    GraphDatabaseHelpers.DeleteDislike(user.userID, postId);
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
                success = GraphDatabaseHelpers.AddToWishList(postID, user.userID);
            }
            return Json(success);
        }

        public JsonResult AddWishPlace(int placeID, int userID)
        {
            User user = UserHelpers.GetCurrentUser(Session);
            bool success = false;
            if (user != null)
            {
                success = GraphDatabaseHelpers.AddToWishlist(placeID, userID);
            }
            return Json(success);
        }

        public JsonResult AddFriend(int userID, int otherUserID)
        {
            User user = UserHelpers.GetCurrentUser(Session);
            bool success = false;
            if (user != null)
            {
                success = GraphDatabaseHelpers.AddFriend(userID, otherUserID);
            }
            return Json(success);
        }

        public JsonResult DeclineRequestFriend(int userID, int otherUserID)
        {
            User user = UserHelpers.GetCurrentUser(Session);
            bool success = false;
            if (user != null)
            {
                success = GraphDatabaseHelpers.DeclineRequestFriend(userID, otherUserID);
            }
            return Json(success);
        }

        public JsonResult SendRequestFriend(int userID, int otherUserID)
        {
            User user = UserHelpers.GetCurrentUser(Session);
            bool success = false;
            if (user != null)
            {
                success = GraphDatabaseHelpers.SendRequestFriend(userID, otherUserID);
            }
            return Json(success);
        }

        public JsonResult Unfriend(int userID, int otherUserID)
        {
            User user = UserHelpers.GetCurrentUser(Session);
            bool success = false;
            if (user != null)
            {
                success = GraphDatabaseHelpers.Unfriend(userID, otherUserID);
            }
            return Json(success);
        }

        public JsonResult RemoveFromWishlist(int postID)
        {
            User user = UserHelpers.GetCurrentUser(Session);
            bool success = false;
            if (user != null)
            {
                success = GraphDatabaseHelpers.RemoveFromWishList(postID, user.userID);
            }
            return Json(success);
        }

        public JsonResult RemoveWishPlace(int placeID, int userID)
        {
            User user = UserHelpers.GetCurrentUser(Session);
            bool success = false;
            if (user != null)
            {
                success = GraphDatabaseHelpers.RemoveFromWishlist(placeID, userID);
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
                string conversationID = "";
                if (user.userID < friendID)
                {
                    conversationID = user.userID + "_" + friendID;
                }
                else
                {
                    conversationID = friendID + "_" + user.userID;
                }
                listMessage = GraphDatabaseHelpers.GetListMessage(conversationID, 10);
                for (int i = 0; i < listMessage.Count; i++)
                {
                    listUser.Add(GraphDatabaseHelpers.FindUser(listMessage[i]));
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
                string conversationID = "";
                if (user.userID < friendID)
                {
                    conversationID = user.userID + "_" + friendID;
                }
                else
                {
                    conversationID = friendID + "_" + user.userID;
                }
                message = GraphDatabaseHelpers.CreateMessage(conversationID, content, user.userID, friendID);

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

                success = GraphDatabaseHelpers.EditProfile(user);
            }
            return Json(success);
        }
        public JsonResult ReportPost(int postID, int userReportID, int userReportedID)
        {
            bool success = false;

            ReportPost reportPost = new ReportPost();
            reportPost.postID = postID;
            reportPost.content = "Khong thich";
            reportPost.userReportID = userReportID;
            reportPost.userReportedID = userReportedID;

            GraphDatabaseHelpers.InsertReportPost(reportPost);

            return Json(success);
        }

        public JsonResult ReportUser(int userReportID, int userReportedID)
        {
            bool success = false;

            ReportUser reportUser = new ReportUser();
            reportUser.content = "Khong thich";
            reportUser.userReportID = userReportID;
            reportUser.userReportedID = userReportedID;

            GraphDatabaseHelpers.InsertReportUser(reportUser);

            return Json(success);
        }
	}
}