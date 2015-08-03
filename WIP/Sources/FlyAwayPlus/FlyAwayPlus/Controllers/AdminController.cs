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
            List<User> listAllUsers = new List<User>();
            listAllUsers = GraphDatabaseHelpers.Instance.ListAllUsers();
            ViewData["listAllUsers"] = listAllUsers;
            return View();
        }
	}
}