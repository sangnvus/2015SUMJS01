using FlyAwayPlus.Helpers;
using System;
using System.Collections.Generic;
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
        public ActionResult Index()
        {
            return View();
        }


        public ActionResult RoomDetail(int roomID = 0)
        {
            User user = UserHelpers.GetCurrentUser(Session);
            if (user == null)
            {
                return RedirectToAction("Index", "Home");
            }
            List<Post> listPost = GraphDatabaseHelpers.Instance.FindPostInRoom(roomID, 0);
            User admin = GraphDatabaseHelpers.Instance.FindAdminInRoom(roomID);
            List<User> listUserInRoom = GraphDatabaseHelpers.Instance.FindUserInRoom(roomID);
            List<User> listUserRequestJoinRoom = GraphDatabaseHelpers.Instance.FindUserRequestJoinRoom(roomID);
            List<Message> listMessage = GraphDatabaseHelpers.Instance.GetListMessageInRoom(roomID,0);
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
            ViewData["roomID"] = roomID;
            return View();
        }

        public ActionResult RoomList()
        {
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
	}
}