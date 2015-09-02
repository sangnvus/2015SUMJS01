using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using FlyAwayPlus.Helpers;
using FlyAwayPlus.Models;
using Microsoft.Ajax.Utilities;

namespace FlyAwayPlus.Controllers
{
    public class PostController : Controller
    {
        //
        // GET: /PostDetail/
        public ActionResult Index(int id = 0, int userId = 0, string activity = "" )
        {
            Post post;
            User user = UserHelpers.GetCurrentUser(Session);
            User userPost;
            List<Comment> listComment;
            List<User> listSuggestFriend;
            List<User> listFriend;
            List<string> listFriendType = new List<string>();
            List<int> listMutualFriends = new List<int>();

            List<Place> listSuggestPlace;
            List<bool> listIsVisitedPlace = new List<bool>();
            List<int> listNumberOfPost = new List<int>();
            List<bool> checkWishlist = new List<bool>();

            List<Photo> listPhoto = new List<Photo>();
            Dictionary<int, User> dict = new Dictionary<int, User>();
            Video video = new Video();
            int likeCount = 0;
            int dislikeCount = 0;
            int userComment = 0;
            string placeName;

            if (user == null)
            {
                return RedirectToAction("Index", "Home");
            }

            try
            {
                post = GraphDatabaseHelpers.Instance.FindPost(id, user);
                if (post == null || post.Privacy == FapConstants.PostLock)
                {
                    return RedirectToAction("Index", "Home");
                }

                if (userId != 0 && !activity.Equals(String.Empty))
                {
                    GraphDatabaseHelpers.Instance.ViewNotification(id, userId, activity);
                }

                placeName = GraphDatabaseHelpers.Instance.FindPlace(post).Name;
                userPost = GraphDatabaseHelpers.Instance.SearchUser(id);
                listFriend = GraphDatabaseHelpers.Instance.GetListFriend(user.UserId);
                listComment = GraphDatabaseHelpers.Instance.FindComment(id);

                likeCount = GraphDatabaseHelpers.Instance.CountLike(id);
                dislikeCount = GraphDatabaseHelpers.Instance.CountDislike(id);
                userComment = GraphDatabaseHelpers.Instance.CountUserComment(id);

                listPhoto = GraphDatabaseHelpers.Instance.FindPhoto(id);
                video = GraphDatabaseHelpers.Instance.FindVideo(id);
                foreach (var comment in listComment)
                {
                    dict.Add(comment.CommentId, GraphDatabaseHelpers.Instance.FindUser(comment));
                }

                listSuggestFriend = GraphDatabaseHelpers.Instance.SuggestFriend(user.UserId);
                foreach (var otherUser in listSuggestFriend)
                {
                    listFriendType.Add(GraphDatabaseHelpers.Instance.GetFriendType(user.UserId, otherUser.UserId));
                    listMutualFriends.Add(GraphDatabaseHelpers.Instance.CountMutualFriend(user.UserId, otherUser.UserId));
                }

                listSuggestPlace = GraphDatabaseHelpers.Instance.SuggestPlace();
                foreach (var otherPlace in listSuggestPlace)
                {
                    listIsVisitedPlace.Add(GraphDatabaseHelpers.Instance.IsVisitedPlace(user.UserId, otherPlace.PlaceId));
                    listNumberOfPost.Add(GraphDatabaseHelpers.Instance.NumberOfPost(otherPlace.PlaceId));
                    checkWishlist.Add(GraphDatabaseHelpers.Instance.IsInWishist(otherPlace.PlaceId, user.UserId));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return RedirectToAction("Index", "Home");
            }

            ViewData["video"] = video;
            ViewData["userPost"] = userPost;
            ViewData["user"] = user;
            ViewData["listComment"] = listComment;
            ViewData["dict"] = dict;
            ViewData["likeCount"] = likeCount;
            ViewData["dislikeCount"] = dislikeCount;
            ViewData["userComment"] = userComment;
            ViewData["listPhoto"] = listPhoto;
            ViewData["listSuggestFriend"] = listSuggestFriend;
            ViewData["listFriendType"] = listFriendType;
            ViewData["listMutualFriends"] = listMutualFriends;

            ViewData["listSuggestPlace"] = listSuggestPlace;
            ViewData["listIsVisitedPlace"] = listIsVisitedPlace;
            ViewData["listNumberOfPost"] = listNumberOfPost;
            ViewData["checkWishlist"] = checkWishlist;
            ViewData["listFriend"] = listFriend;
            ViewData["placeName"] = placeName;
            return View(post);
        }

        public RedirectToRouteResult Add(FormCollection form)
        {
            string message = Request.Form["message"];
            Double latitude = double.Parse(Request.Form["lat"]);
            Double longitude = double.Parse(Request.Form["lng"]);
            string location = Request.Form["name"];
            string address = Request.Form["formatted_address"];
            List<string> images = Request.Form["uploadedimages"].Split('#').ToList();
            images.RemoveAt(0);
            string privacy = Request.Form["privacy"];
            string uploadedVideoYoutubeId = Request.Form["uploadedvideo"];

            Post newPost = new Post
            {
                Content = message,
                DateCreated = DateTime.Now.ToString(FapConstants.DatetimeFormat),
                Privacy = privacy
            };

            Place newPlace = new Place
            {
                Name = location  + " - " + address,
                Latitude = latitude,
                Longitude = longitude
            };
            Video newVideo = null;
            if (!uploadedVideoYoutubeId.IsNullOrWhiteSpace())
            {
                newVideo = new Video
                {
                    Path = uploadedVideoYoutubeId,
                    DateCreated = DateTime.Now.ToString(FapConstants.DatetimeFormat)
                };
            }

            List<Photo> newPhotos = images.Select(img => new Photo { DateCreated = DateTime.Now.ToString(FapConstants.DatetimeFormat), Url = img }).ToList();

            var user = (User)Session["user"];

            GraphDatabaseHelpers.Instance.InsertPost(user, ref newPost, newPhotos, newPlace, newVideo);

            return RedirectToAction("Index", "Home");
        }

        public void DeletePost(int postId)
        {
            GraphDatabaseHelpers.Instance.DeletePost(postId);
        }
        public void EditPost(int postId, string newContent)
        {
            GraphDatabaseHelpers.Instance.EditPost(postId, newContent);
        }
    }
}