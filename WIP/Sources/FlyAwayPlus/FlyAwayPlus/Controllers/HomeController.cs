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
        public ActionResult Index()
        {
            User user = UserHelpers.getCurrentUser(Session);
            

            List<Post> listPost = new List<Post>();
            Photo photo = null;
            Place place = null;
            User userDict = null;
            Dictionary<int, Photo> listPhotoDict = new Dictionary<int, Photo>();
            Dictionary<int, Place> listPlaceDict = new Dictionary<int, Place>();
            Dictionary<int, User> listUserDict = new Dictionary<int, User>();

            if (user == null)
            {
                /*
                 * Search all public post
                 */
                listPost = GraphDatabaseHelpers.searchAllPost();
                foreach (Post po in listPost)
                {
                    photo = GraphDatabaseHelpers.findPhoto(po);
                    place = GraphDatabaseHelpers.findPlace(po);
                    userDict = GraphDatabaseHelpers.findUser(po);

                    listPhotoDict.Add(po.postID, photo);
                    listPlaceDict.Add(po.postID, place);
                    listUserDict.Add(po.postID, userDict);
                }

            }
            else
            {
                GraphClient client = new GraphClient(new Uri(ConfigurationManager.AppSettings["dbGraphUri"]));
                client.Connect();

                listPost = GraphDatabaseHelpers.findPostFollowing(user);

                foreach (Post po in listPost)
                {
                    photo = GraphDatabaseHelpers.findPhoto(po);
                    place = GraphDatabaseHelpers.findPlace(po);
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