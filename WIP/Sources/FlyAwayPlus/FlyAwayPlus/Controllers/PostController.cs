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
        public ActionResult Index(int id = 0)
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

            Photo photo;
            Dictionary<int, User> dict = new Dictionary<int, User>();
            int likeCount = 0;
            int dislikeCount = 0;
            int userComment = 0;

            if (user == null)
            {
                return RedirectToAction("Index", "Home");
            }

            try
            {
                post = GraphDatabaseHelpers.Instance.FindPost(id, user);
                if (post == null)
                {
                    return RedirectToAction("Index", "Home");
                }

                userPost = GraphDatabaseHelpers.Instance.SearchUser(post.postID);
                listFriend = GraphDatabaseHelpers.Instance.GetListFriend(user.userID);
                listComment = GraphDatabaseHelpers.Instance.FindComment(post);

                likeCount = GraphDatabaseHelpers.Instance.CountLike(post.postID);
                dislikeCount = GraphDatabaseHelpers.Instance.CountDislike(post.postID);
                userComment = GraphDatabaseHelpers.Instance.CountUserComment(post.postID);

                // TODO: Change to list of photos.
                photo = GraphDatabaseHelpers.Instance.FindPhoto(post.postID).FirstOrDefault();
                foreach (var comment in listComment)
                {
                    dict.Add(comment.commentID, GraphDatabaseHelpers.Instance.FindUser(comment));
                }

                listSuggestFriend = GraphDatabaseHelpers.Instance.SuggestFriend(user.userID);
                foreach (var otherUser in listSuggestFriend)
                {
                    listFriendType.Add(GraphDatabaseHelpers.Instance.GetFriendType(user.userID, otherUser.userID));
                    listMutualFriends.Add(GraphDatabaseHelpers.Instance.CountMutualFriend(user.userID, otherUser.userID));
                }

                listSuggestPlace = GraphDatabaseHelpers.Instance.SuggestPlace();
                foreach (var otherPlace in listSuggestPlace)
                {
                    listIsVisitedPlace.Add(GraphDatabaseHelpers.Instance.IsVisitedPlace(user.userID, otherPlace.placeID));
                    listNumberOfPost.Add(GraphDatabaseHelpers.Instance.NumberOfPost(otherPlace.placeID));
                    checkWishlist.Add(GraphDatabaseHelpers.Instance.IsInWishist(otherPlace.placeID, user.userID));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return RedirectToAction("Index", "Home");
            }

            ViewData["userPost"] = userPost;
            ViewData["user"] = user;
            ViewData["listComment"] = listComment;
            ViewData["dict"] = dict;
            ViewData["likeCount"] = likeCount;
            ViewData["dislikeCount"] = dislikeCount;
            ViewData["userComment"] = userComment;
            ViewData["photo"] = photo;
            ViewData["listSuggestFriend"] = listSuggestFriend;
            ViewData["listFriendType"] = listFriendType;
            ViewData["listMutualFriends"] = listMutualFriends;

            ViewData["listSuggestPlace"] = listSuggestPlace;
            ViewData["listIsVisitedPlace"] = listIsVisitedPlace;
            ViewData["listNumberOfPost"] = listNumberOfPost;
            ViewData["checkWishlist"] = checkWishlist;
            ViewData["listFriend"] = listFriend;
            return View(post);
        }

        public RedirectToRouteResult Add(FormCollection form)
        {
            string message = Request.Form["message"];
            Decimal latitude = Decimal.Parse(Request.Form["lat"]);
            Decimal longitude = Decimal.Parse(Request.Form["lng"]);
            string location = Request.Form["formatted_address"];
            List<string> images = Request.Form["uploadedimages"].Split('#').ToList();
            images.RemoveAt(0);
            string privacy = Request.Form["privacy"];
            string uploadedVideoYoutubeId = Request.Form["uploadedvideo"];

            Post newPost = new Post
            {
                content = message,
                dateCreated = DateTime.Now.ToString(FapConstants.DatetimeFormat),
                privacy = privacy
            };

            Place newPlace = new Place
            {
                name = location,
                latitude = latitude,
                longitude = longitude
            };
            Video newVideo = null;
            if (!uploadedVideoYoutubeId.IsNullOrWhiteSpace())
            {
                newVideo = new Video
                {
                    path = uploadedVideoYoutubeId,
                    dateCreated = DateTime.Now.ToString(FapConstants.DatetimeFormat)
                };
            }

            List<Photo> newPhotos = images.Select(img => new Photo { dateCreated = DateTime.Now.ToString(FapConstants.DatetimeFormat), url = img }).ToList();

            var user = (User)Session["user"];

            GraphDatabaseHelpers.Instance.InsertPost(user, newPost, newPhotos, newPlace, newVideo);

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