using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FlyAwayPlus.Models;
using FlyAwayPlus.Helpers;
namespace FlyAwayPlus.Controllers
{
    public class UserController : Controller
    {
        //
        // GET: /User/
        public ActionResult Index(int id = 0)
        {
            User userSession = UserHelpers.GetCurrentUser(Session);
            User user;
            List<Post> listPost;
            List<Photo> listPhoto = new List<Photo>();
            List<User> friend;
            List<String> timeline;
            Photo photo;
            Place place;
            Dictionary<int, Photo> listPhotoDict = new Dictionary<int, Photo>();
            Dictionary<int, Place> listPlaceDict = new Dictionary<int, Place>();
            Dictionary<String, List<Post>> listPostDict = new Dictionary<string, List<Post>>();
            
            if (userSession == null)
            {
                return RedirectToAction("Index", "Home");
            }

            if (userSession.userID == id || id == 0)
            {
                user = userSession;
                listPost = GraphDatabaseHelpers.FindPostOfUser(userSession);
                friend = GraphDatabaseHelpers.FindFriend(userSession);
                timeline = DateHelpers.toTimeLineDate(listPost, listPostDict);

                foreach (Post po in listPost)
                {
                    photo = GraphDatabaseHelpers.FindPhoto(po);
                    place = GraphDatabaseHelpers.FindPlace(po);

                    listPhotoDict.Add(po.postID, photo);
                    listPlaceDict.Add(po.postID, place);
                }
            }
            else
            {
                user = GraphDatabaseHelpers.findUser(id);
                if (user == null)
                {
                    return RedirectToAction("Index", "Home");
                }
                listPost = GraphDatabaseHelpers.FindPostOfOtherUser(userSession, user);
                friend = GraphDatabaseHelpers.FindFriend(user);
                timeline = DateHelpers.toTimeLineDate(listPost, listPostDict);

                foreach (Post po in listPost)
                {
                    photo = GraphDatabaseHelpers.FindPhoto(po);
                    place = GraphDatabaseHelpers.FindPlace(po);

                    listPhotoDict.Add(po.postID, photo);
                    listPlaceDict.Add(po.postID, place);
                }
            }

            ViewData["listPostDict"] = listPostDict;
            ViewData["listPhotoDict"] = listPhotoDict;
            ViewData["listPlaceDict"] = listPlaceDict;
            ViewData["timeline"] = timeline;
            ViewData["friend"] = friend;

            return View(user);
        }
	}
}