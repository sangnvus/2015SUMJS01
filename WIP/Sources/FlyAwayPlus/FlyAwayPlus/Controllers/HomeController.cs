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

        public ActionResult LoadMore()
        {
            int totalAdd = 0;
            int pageNumber = 1;

            if (Session["totalAddPost"] == null)
            {
                Session["totalAddPost"] = 0;
            }
            else
            {
                totalAdd = int.Parse(Session["totalAddPost"].ToString());
            }

            if (Session["pageNumberHomeIndex"] == null)
            {
                Session["pageNumberHomeIndex"] = 1;
            }
            else
            {
                pageNumber = int.Parse(Session["pageNumberHomeIndex"].ToString());
            }
            int skip = pageNumber * RecordsPerPage + totalAdd;
            
            loadData(skip);

            Session["pageNumberHomeIndex"] = pageNumber + 1;
            return PartialView("_listPost");
        }

        private void loadData(int skip)
        {
            List<Post> listPost = new List<Post>();
            Dictionary<int, Photo> listPhotoDict = new Dictionary<int, Photo>();
            Dictionary<int, Place> listPlaceDict = new Dictionary<int, Place>();
            Dictionary<int, User> listUserDict = new Dictionary<int, User>();
            Photo photo;
            Place place;
            User userDict;
            User user = UserHelpers.GetCurrentUser(Session);

            if (user == null)
            {
                /*
                 * Search limit public post
                 */
                listPost = GraphDatabaseHelpers.SearchLimitPost(skip, RecordsPerPage);
                foreach (Post po in listPost)
                {
                    photo = GraphDatabaseHelpers.FindPhoto(po);
                    place = GraphDatabaseHelpers.FindPlace(po);
                    userDict = GraphDatabaseHelpers.findUser(po);

                    listPhotoDict.Add(po.postID, photo);
                    listPlaceDict.Add(po.postID, place);
                    listUserDict.Add(po.postID, userDict);
                }
            }
            else
            {
                /**
                 * Search limit following post
                 */
                listPost = GraphDatabaseHelpers.FindLimitPostFollowing(user, skip, RecordsPerPage);

                foreach (Post po in listPost)
                {
                    photo = GraphDatabaseHelpers.FindPhoto(po);
                    place = GraphDatabaseHelpers.FindPlace(po);
                    userDict = GraphDatabaseHelpers.findUser(po);

                    listPhotoDict.Add(po.postID, photo);
                    listPlaceDict.Add(po.postID, place);
                    listUserDict.Add(po.postID, userDict);
                }
            }

            ViewData["listPost"] = listPost;
            ViewData["listPhotoDict"] = listPhotoDict;
            ViewData["listPlaceDict"] = listPlaceDict;
            ViewData["listUserDict"] = listUserDict;

            if (listPost.Count < RecordsPerPage)
            {
                ViewData["isLoadMore"] = "false";
            }
            else
            {
                ViewData["isLoadMore"] = "true";
            }
        }
        public ActionResult Index()
        {
            loadData(0);
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