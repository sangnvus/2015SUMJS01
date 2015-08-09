using FlyAwayPlus.Helpers;
using FlyAwayPlus.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FlyAwayPlus.Controllers
{
    public class SearchController : Controller
    {
        //
        // GET: /Search/
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Search(string keyword = "")
        {
            List<User> listUser = GraphDatabaseHelpers.Instance.SearchUserByKeyword(keyword.ToUpper());
            List<Place> listPlace = GraphDatabaseHelpers.Instance.SearchPlaceByKeyword(keyword.ToUpper());
            Dictionary<int, List<Photo>> listPhotoDict = new Dictionary<int, List<Photo>>();
            Dictionary<int, int> numberOfPostDict = new Dictionary<int, int>();
            Dictionary<int, bool> wishlist = new Dictionary<int, bool>();
            User user = UserHelpers.GetCurrentUser(Session);

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


            ViewData["listUser"] = listUser;
            ViewData["listPlace"] = listPlace;
            ViewData["keyword"] = keyword;
            ViewData["listPhotoDict"] = listPhotoDict;
            ViewData["numberOfPostDict"] = numberOfPostDict;
            ViewData["wishlist"] = wishlist;
            return View();
        }

        public ActionResult SearchDetail()
        {
            return View();
        }
    }
}