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
            List<User> listUser = GraphDatabaseHelpers.Instance.searchUserByKeyword(keyword.ToUpper());
            List<Place> listPlace = GraphDatabaseHelpers.Instance.searchPlaceByKeyword(keyword.ToUpper());

            ViewData["listUser"] = listUser;
            ViewData["listPlace"] = listPlace;
            ViewData["keyword"] = keyword;
            return View();
        }

        public ActionResult SearchDetail()
        {
            return View();
        }
    }
}