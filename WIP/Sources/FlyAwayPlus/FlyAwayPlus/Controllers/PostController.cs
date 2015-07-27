using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using FlyAwayPlus.Helpers;
using FlyAwayPlus.Helpers.UploadImage;
using FlyAwayPlus.Models;

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
            List<User> listSuggestFriend = new List<User>();
            List<string> listFriendType = new List<string>();
            List<int> listMutualFriends = new List<int>();

            List<Place> listSuggestPlace = new List<Place>();
            List<bool> listIsVisitedPlace = new List<bool>();
            List<int> listNumberOfPost = new List<int>();
            List<bool> checkWishlist = new List<bool>();

            Photo photo = new Photo();
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
                post = GraphDatabaseHelpers.FindPost(id, user);
                if (post == null)
                {
                    return RedirectToAction("Index", "Home");
                }

                userPost = GraphDatabaseHelpers.SearchUser(post);
                listComment = GraphDatabaseHelpers.FindComment(post);

                likeCount = GraphDatabaseHelpers.CountLike(post.postID);
                dislikeCount = GraphDatabaseHelpers.CountDislike(post.postID);
                userComment = GraphDatabaseHelpers.CountUserComment(post.postID);
                photo = GraphDatabaseHelpers.FindPhoto(post);
                foreach (var comment in listComment)
                {
                    dict.Add(comment.commentID, GraphDatabaseHelpers.FindUser(comment));
                }

                listSuggestFriend = GraphDatabaseHelpers.SuggestFriend(user.userID);
                foreach (var otherUser in listSuggestFriend)
                {
                    listFriendType.Add(GraphDatabaseHelpers.GetFriendType(user.userID, otherUser.userID));
                    listMutualFriends.Add(GraphDatabaseHelpers.CountMutualFriend(user.userID, otherUser.userID));
                }

                listSuggestPlace = GraphDatabaseHelpers.SuggestPlace();
                foreach (var otherPlace in listSuggestPlace)
                {
                    listIsVisitedPlace.Add(GraphDatabaseHelpers.isVisitedPlace(user.userID, otherPlace.placeID));
                    listNumberOfPost.Add(GraphDatabaseHelpers.NumberOfPost(otherPlace.placeID));
                    checkWishlist.Add(GraphDatabaseHelpers.IsInWishist(otherPlace.placeID, user.userID));
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
            return View(post);
        }

        public RedirectToRouteResult Add(FormCollection form)
        {
            string message = Request.Form["message"];
            Decimal latitude = Decimal.Parse(Request.Form["lat"]);
            Decimal longitude = Decimal.Parse(Request.Form["lng"]);
            string location = Request.Form["location"];
            string filename;
            string privacy = Request.Form["privacy"];

            filename = UploadImage(Request.Files);

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

            Photo newPhoto = new Photo
            {
                dateCreated = DateTime.Now.ToString(FapConstants.DatetimeFormat),
                url = filename
            };

            var user = (User)Session["user"];

            GraphDatabaseHelpers.InsertPost(user, newPost, newPhoto, newPlace);

            return RedirectToAction("Index", "Home");
        }

        private string UploadImage(HttpFileCollectionBase files)
        {
            // TODO: Multiple file names.
            string filename = string.Empty;
            foreach (string item in files)
            {
                var file = files[item];
                if (file == null || file.ContentLength == 0)
                    continue;

                if (file.ContentLength <= 0) continue;
                ImageUpload imageUpload = new ImageUpload
                {
                    Width = FapConstants.UploadedImageMaxWidthPixcel,
                    Height = FapConstants.UploadedImageMaxHeightPixcel
                };
                ImageResult imageResult = imageUpload.RenameUploadFile(file);

                if (imageResult.Success)
                {
                    filename = imageResult.ImageName;
                }
                else
                {
                    // TODO: ERROR message.
                    ViewBag.Error = imageResult.ErrorMessage;
                }
            }
            return filename;
        }
    }
}