using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FlyAwayPlus.Helpers;
using FlyAwayPlus.Models;

namespace FlyAwayPlus.Controllers
{
    public class AdminController : Controller
    {
        //
        // GET: /Admin/
        public ActionResult Index()
        {
            return View();
        }

        public JsonResult LockUser(int userId)
        {
            bool success = false;
            GraphDatabaseHelpers.Instance.LockUser(userId);

            return Json(success);
        }

        public JsonResult UnlockUser(int userId)
        {
            bool success = false;
            GraphDatabaseHelpers.Instance.UnlockUser(userId);

            return Json(success);
        }

        public ActionResult Main()
        {
            List<User> listAllUsers = new List<User>();
            listAllUsers = GraphDatabaseHelpers.Instance.ListAllUsers();
            ViewData["listAllUsers"] = listAllUsers;
            return View();
        }
    }
}