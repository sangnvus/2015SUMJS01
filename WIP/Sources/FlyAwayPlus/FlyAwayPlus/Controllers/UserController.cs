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
        //
        // GET: /User/
        public ActionResult Index(int id = 0)
        {
            User userSession = UserHelpers.GetCurrentUser(Session);
            User user;
            List<Post> listPost;
            List<Photo> listPhoto = new List<Photo>();
            List<User> friend;
            List<String> timeline;
            Photo photo;
            Place place;
            Dictionary<int, Photo> listPhotoDict = new Dictionary<int, Photo>();
            Dictionary<int, Place> listPlaceDict = new Dictionary<int, Place>();
            Dictionary<String, List<Post>> listPostDict = new Dictionary<string, List<Post>>();
            
            if (userSession == null)
            {
                return RedirectToAction("Index", "Home");
            }

            if (userSession.userID == id || id == 0)
            {
                user = userSession;
                listPost = GraphDatabaseHelpers.FindPostOfUser(userSession);
                friend = GraphDatabaseHelpers.FindFriend(userSession);
                timeline = DateHelpers.toTimeLineDate(listPost, listPostDict);

                foreach (Post po in listPost)
                {
                    photo = GraphDatabaseHelpers.FindPhoto(po);
                    place = GraphDatabaseHelpers.FindPlace(po);

                    listPhotoDict.Add(po.postID, photo);
                    listPlaceDict.Add(po.postID, place);
                }
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
                timeline = DateHelpers.toTimeLineDate(listPost, listPostDict);

                foreach (Post po in listPost)
                {
                    photo = GraphDatabaseHelpers.FindPhoto(po);
                    place = GraphDatabaseHelpers.FindPlace(po);

                    listPhotoDict.Add(po.postID, photo);
                    listPlaceDict.Add(po.postID, place);
                }
            }

            ViewData["listPostDict"] = listPostDict;
            ViewData["listPhotoDict"] = listPhotoDict;
            ViewData["listPlaceDict"] = listPlaceDict;
            ViewData["timeline"] = timeline;
            ViewData["friend"] = friend;

            return View(user);
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

        public JsonResult Like(int postId)
        {
            User user = UserHelpers.GetCurrentUser(Session);
            Boolean a = false;
            if (user != null)
            {
                int like = GraphDatabaseHelpers.FindLike(user.userID, postId);
                if (like == 0)
                {
                    // User like post and delete exist dislike
                    a = GraphDatabaseHelpers.InsertLike(user.userID, postId);
                    GraphDatabaseHelpers.DeleteDislike(user.userID, postId);
                }
                else
                {
                    // delete exist like post
                    GraphDatabaseHelpers.DeleteLike(user.userID, postId);
                }
            }
            return Json(a);
        }

        public JsonResult Dislike(int postId)
        {
            User user = UserHelpers.GetCurrentUser(Session);
            Boolean a = false;
            if (user != null)
            {
                int dislike = GraphDatabaseHelpers.FindDislike(user.userID, postId);
                if (dislike == 0)
                {
                    // user dislike post and delete exist like
                    a = GraphDatabaseHelpers.InsertDislike(user.userID, postId);
                    GraphDatabaseHelpers.DeleteLike(user.userID, postId);
                }
                else
                {
                    // delete exist dislike
                    GraphDatabaseHelpers.DeleteDislike(user.userID, postId);
                }
            }
            return Json(a);
        }
	}
}