using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using FlyAwayPlus.Helpers;
using FlyAwayPlus.Models;
using PagedList;

namespace FlyAwayPlus.Controllers
{
    public class AdminController : Controller
    {
        [HttpPost]
        public RedirectToRouteResult Authenticate()
        {
            string username = Request["username"];
            string password = Request["password"];
            if (username.Equals("admin") && password.Equals("admin"))
            {
                Session["administrator"] = "administrator";
                return RedirectToAction("Main", "Admin", new
                {
                    tab = ""
                });
            }
            else
            {
                return RedirectToAction("Index", "Admin");
            }
            // Defined admin username & password
            // Get Post data
            // Check data == admin username & password?
            // if true:
            //    Set cookie/session authen=true
            //    return Redirect to action main
            //

        }

        //
        // GET: /Admin/
        public ActionResult Index()
        {
            Session.Abandon();
            return View();
        }

        public ActionResult LogOut()
        {
            FormsAuthentication.SignOut();
            Session.Abandon(); // it will clear the session at the end of request
            return RedirectToAction("Index", "Admin");
        }

        public ActionResult Main(int? page, string tab)
        {
            if (Session["administrator"] == null || Session["administrator"].Equals(""))
            {
                return RedirectToAction("Index", "Admin");
            }
            else
            {
                List<User> listAllUsers = GraphDatabaseHelpers.Instance.ListAllUsers();
                List<GraphDatabaseHelpers.ReportPost> listAllReportPost = GraphDatabaseHelpers.Instance.ListAllReportPosts();
                List<GraphDatabaseHelpers.ReportUser> listAllReportUser = GraphDatabaseHelpers.Instance.ListAllReportUsers();
                int numberOfUser = GraphDatabaseHelpers.Instance.NumberOfUser();
                int numberOfReportPost = GraphDatabaseHelpers.Instance.NumberOfReportPost();
                int numberOfReportUser = GraphDatabaseHelpers.Instance.NumberOfReportUser();


                int pageSize = 3;
                int pageNumber = (page ?? 1);
                var onePageOfUsers = listAllUsers.ToPagedList(pageNumber, pageSize);
                var onePageOfReportPosts = listAllReportPost.ToPagedList(pageNumber, pageSize);
                var onePageOfReportUsers = listAllReportUser.ToPagedList(pageNumber, pageSize);
                ViewBag.OnePageOfUsers = onePageOfUsers;
                ViewBag.OnePageOfReportPosts = onePageOfReportPosts;
                ViewBag.OnePageOfReportUsers = onePageOfReportUsers;
                ViewBag.Tab = tab;
                ViewBag.NumberOfUser = numberOfUser;
                ViewBag.NumberOfReportPost = numberOfReportPost;
                ViewBag.NumberOfReportUser = numberOfReportUser;
                return View();
            }
        }

        public JsonResult LockUser(int userId)
        {
            GraphDatabaseHelpers.Instance.LockUser(userId);
            return Json(true);
        }

        public JsonResult LockReportUser(int userReportedID, int userReportID)
        {
            GraphDatabaseHelpers.Instance.LockUser(userReportedID);
            GraphDatabaseHelpers.Instance.DeleteReportUser(userReportedID, userReportID);
            return Json(true);
        }

        public JsonResult UnlockUser(int userId)
        {
            bool success = false;
            GraphDatabaseHelpers.Instance.UnlockUser(userId);

            return Json(success);
        }

        public JsonResult LockReportPost(int postId, int userReportID)
        {
            try
            {
                GraphDatabaseHelpers.Instance.LockPost(postId);
                GraphDatabaseHelpers.Instance.DeleteReportPost(postId, userReportID);
                return Json(true);
            }
            catch (Exception exception)
            {
                return Json(false);
            }
        }

        public JsonResult CancelReportPost(int postId, int userReportID)
        {
            try
            {
                GraphDatabaseHelpers.Instance.DeleteReportPost(postId, userReportID);
                return Json(true);
            }
            catch (Exception exception)
            {
                return Json(false);
            }
        }

        public JsonResult CancelReportUser(int userReportedID, int userReportID)
        {
            try
            {
                GraphDatabaseHelpers.Instance.DeleteReportUser(userReportedID, userReportID);
                return Json(true);
            }
            catch (Exception exception)
            {
                return Json(false);
            }
        }

        public ActionResult ViewPost(int id)
        {
            try
            {
                Post post = GraphDatabaseHelpers.Instance.FindPostByID(id);
                User userReported = GraphDatabaseHelpers.Instance.FindUser(post);

                List<Photo> photo = GraphDatabaseHelpers.Instance.FindPhoto(id);
                List<Comment> listComment = GraphDatabaseHelpers.Instance.FindComment(id);
                ViewData["post"] = post;
                ViewData["user"] = userReported;
                ViewData["photo"] = photo;
                ViewData["listComment"] = listComment;
                return View();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return View();
            }
        }
    }
}