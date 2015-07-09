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
            Photo Photo = null;
            Place Place = null;
            Dictionary<string, Photo> listPhotoDict = new Dictionary<string, Photo>();
            Dictionary<string, Place> listPlaceDict = new Dictionary<string, Place>();

            if (user == null)
            {
                // TODO
            }
            else
            {
                GraphClient client = new GraphClient(new Uri(ConfigurationManager.AppSettings["dbGraphUri"]));
                client.Connect();

                listPost = client.Cypher.Match("(u:user {userID:" + user.userID + "})-[:has]->(p:post)")
                            .Return<Post>("p").Results.OrderBy(p => p.postID).ToList();

                foreach (Post po in listPost)
                {
                    Photo = client.Cypher.Match("(po:post {postID:" + po.postID + "})-[:has]->(ph:photo)")
                            .Return<Photo>("ph").Results.OrderBy(ph => ph.photoID).ToList().First();

                    Place = client.Cypher.Match("(po:post {postID:" + po.postID + "})-[:at]->(pl:place)")
                            .Return<Place>("pl").Results.OrderBy(pl => pl.placeID).ToList().First();

                    listPhotoDict.Add(po.postID.ToString(), Photo);
                    listPlaceDict.Add(po.postID.ToString(), Place);
                }
            }

            ViewData["listPost"] = listPost;
            ViewData["listPhotoDict"] = listPhotoDict;
            ViewData["listPlaceDict"] = listPlaceDict;
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