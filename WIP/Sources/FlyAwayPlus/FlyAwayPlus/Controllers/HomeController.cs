using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FlyAwayPlus.Models;
using Neo4jClient;
using System.Configuration;
using FlyAwayPlus.Helpers;

namespace FlyAwayPlus.Controllers
{
    public class HomeController : Controller
    {
        public const int RecordsPerPage = 10;

        public ActionResult LoadMore(int pageNumber = 0)
        {
            int totalAdd = 0;

            if (Session["totalAddPost"] == null)
            {
                Session["totalAddPost"] = 0;
            }
            else
            {
                totalAdd = int.Parse(Session["totalAddPost"].ToString());
            }

            int skip = pageNumber * RecordsPerPage + totalAdd;
            
            loadData(skip);
            return PartialView("_ListPost");
        }

        private void loadData(int skip)
        {
            List<Post> listPost = new List<Post>();
            User user = UserHelpers.GetCurrentUser(Session);

            if (user == null)
            {
                /*
                 * Search limit public post
                 */
                listPost = GraphDatabaseHelpers.SearchLimitPost(skip, RecordsPerPage);
                FindRelatedInformationPost(listPost);
            }
            else
            {
                /**
                 * Search limit following post
                 */
                listPost = GraphDatabaseHelpers.FindLimitPostFollowing(user, skip, RecordsPerPage);
                FindRelatedInformationPost(listPost);
            }
            
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

        public ActionResult LoadMoreWish(int pageNumber = 0)
        {
            int skip = pageNumber * RecordsPerPage;

            LoadDataWishlist(skip);
            return PartialView("_ListPost");
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

            if (listPost.Count < RecordsPerPage)
            {
                ViewData["isLoadMore"] = "false";
            }
            else
            {
                ViewData["isLoadMore"] = "true";
            }
        }

        private void LoadDataWishlist(int skip)
        {
            User user = UserHelpers.GetCurrentUser(Session);

            if (user != null)
            {
                /**
                 * Search limit following post
                 */
                var listPost = GraphDatabaseHelpers.FindLimitWishlist(user, skip, RecordsPerPage);
                FindRelatedInformationPost(listPost);
            }

            ViewData["typePost"] = "wish";
        }
        public ActionResult Index(string type = "")
        {
            if (type.Equals("wishlist"))
            {
                LoadDataWishlist(0);
            }
            else if(type.Equals("hot")) {
                ViewData["typePost"] = "hot";
            }
            else 
            {
                loadData(0);
            }
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}