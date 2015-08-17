using FlyAwayPlus.Helpers;
using FlyAwayPlus.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;

namespace FlyAwayPlus.Controllers
{
    public class SearchController : Controller
    {
        //
        // GET: /Search/
        public ActionResult Index()
        {
            return Search();
        }

        public ActionResult Search(string keyword = "")
        {
            User user = UserHelpers.GetCurrentUser(Session);
            if (user == null)
            {
                return RedirectToAction("Index", "Home");
            }

            List<User> listUser = GraphDatabaseHelpers.Instance.SearchUserByKeyword(keyword.ToUpper());
            List<Place> listPlace = GraphDatabaseHelpers.Instance.SearchPlaceByKeyword(keyword.ToUpper());
            List<Room> listRoom = GraphDatabaseHelpers.Instance.SearchRoomByKeyword(keyword.ToUpper());
            List<User> listAdminRoom = new List<User>();

            Dictionary<int, List<Photo>> listPhotoDict = new Dictionary<int, List<Photo>>();
            Dictionary<int, int> numberOfPostDict = new Dictionary<int, int>();
            Dictionary<int, bool> wishlist = new Dictionary<int, bool>();

            
            foreach (var room in listRoom)
            {
                listAdminRoom.Add(GraphDatabaseHelpers.Instance.FindAdminInRoom(room.RoomId));
            }

            foreach (var place in listPlace)
            {
                listPhotoDict.Add(place.placeID, GraphDatabaseHelpers.Instance.SearchPhotoInPlace(place.placeID));
                numberOfPostDict.Add(place.placeID, GraphDatabaseHelpers.Instance.CountPostAtPlace(place.placeID));
                if (user == null)
                {
                    wishlist.Add(place.placeID, false);
                }
                else
                {
                    wishlist.Add(place.placeID, GraphDatabaseHelpers.Instance.IsInWishist(place.placeID, user.userID));
                }
            }

            ViewData["listRoom"] = listRoom;
            ViewData["listAdminRoom"] = listAdminRoom;
            ViewData["listUser"] = listUser;
            ViewData["listPlace"] = listPlace;
            ViewData["keyword"] = keyword;
            ViewData["listPhotoDict"] = listPhotoDict;
            ViewData["numberOfPostDict"] = numberOfPostDict;
            ViewData["wishlist"] = wishlist;
            return View();
        }

        public ActionResult SearchDetail(int id = 0)
        {
            List<Post> listPost = new List<Post>();
            List<Place> listPlace = new List<Place>();
            List<Double> distance = new List<double>();
            Place currentPlace = null;
            User user = UserHelpers.GetCurrentUser(Session);

            if (user == null)
            {
                /*
                 * User not login
                 */
                return RedirectToAction("Index", "Home");
            }
            else
            {
                /**
                 * Search limit following post
                 */
                listPost = GraphDatabaseHelpers.Instance.FindPostInPlace(id);
                listPlace = GraphDatabaseHelpers.Instance.SearchPlaceByKeyword("");
                currentPlace = listPlace.Find(item => item.placeID == id);
                listPlace.Remove(currentPlace);
                listPlace.RemoveAll(item => (calculateDistance(currentPlace, item) - 50.0) > 1E-6);

                foreach (var p in listPlace)
                {
                    distance.Add(calculateDistance(currentPlace, p));
                }
                FindRelatedInformationPost(listPost, currentPlace);
            }

            ViewData["currentPlace"] = currentPlace;
            ViewData["listPlace"] = JsonConvert.SerializeObject(listPlace);
            return View();
        }

        public ActionResult LoadPostInPlace(int placeID = 0)
        {
            if (Request.IsAjaxRequest())
            {
                User user = UserHelpers.GetCurrentUser(Session);
                Place currentPlace = GraphDatabaseHelpers.Instance.FindPlaceByID(placeID);
                if (user == null || currentPlace == null)
                {
                    return null;
                }
                List<Post> listPost = new List<Post>();
                listPost = GraphDatabaseHelpers.Instance.FindPostInPlace(placeID);
                FindRelatedInformationPost(listPost, currentPlace);
                return PartialView("_ListPost");
            }
            return null;
        }
        private void FindRelatedInformationPost(List<Post> listPost, Place currentPlace)
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

            foreach (Post po in listPost)
            {
                listPhotoDict.Add(po.postID, GraphDatabaseHelpers.Instance.FindPhoto(po.postID));
                listVideoDict.Add(po.postID, GraphDatabaseHelpers.Instance.FindVideo(po.postID));
                listPlaceDict.Add(po.postID, currentPlace);
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
            if (listPost.Count < FapConstants.RecordsPerPage)
            {
                ViewData["isLoadMore"] = "false";
            }
            else
            {
                ViewData["isLoadMore"] = "true";
            }
        }

        private double calculateDistance(Place p1, Place p2)
        {
            // using formula in http://www.movable-type.co.uk/scripts/latlong.html
            double R = 6371; // distance of the earth in km
            double dLatitude = Radians(p1.latitude - p2.latitude);      // different in Rad of latitude
            double dLongitude = Radians(p1.longitude - p2.longitude);   // different in Rad of longitude

            double a = Math.Sin(dLatitude / 2) * Math.Sin(dLatitude / 2) + Math.Cos(Radians(p1.latitude)) * Math.Cos(Radians(p2.latitude))
                        * Math.Sin(dLongitude / 2) * Math.Sin(dLongitude / 2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1.0 - a));
            return R * c;
        }

        private double Radians(double x)
        {
            return x * Math.PI / 180.0;
        }
    }
}