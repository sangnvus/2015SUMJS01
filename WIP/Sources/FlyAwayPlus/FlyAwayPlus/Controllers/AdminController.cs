using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using FlyAwayPlus.Helpers;
using FlyAwayPlus.Models;

namespace FlyAwayPlus.Controllers
{
    public class AdminController : Controller
    {

        public RedirectToRouteResult Authenticate()
        {
            // Defined admin username & password
            // Get Post data
            // Check data == admin username & password?
            // if true:
            //    Set cookie/session authen=true
            //    return Redirect to action main
            //
            return RedirectToAction("Index", "Admin");
        }

        //
        // GET: /Admin/
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Main()
        {
            List<User> listAllUsers = GraphDatabaseHelpers.Instance.ListAllUsers();
            List<ReportPost> listAllReportPost = GraphDatabaseHelpers.Instance.ListAllReportPosts();
            List<ReportUser> listAllReportUser = GraphDatabaseHelpers.Instance.ListAllReportUsers();
            ViewData["listAllUsers"] = listAllUsers;
            ViewData["listAllReportPost"] = listAllReportPost;
            ViewData["listAllReportUser"] = listAllReportUser;
            return View();
        }

        public JsonResult LockUser(int userId, int reportID)
        {
            GraphDatabaseHelpers.Instance.LockUser(userId);
            GraphDatabaseHelpers.Instance.DeleteReportUser(reportID);
            return Json(true);
        }

        public JsonResult UnlockUser(int userId)
        {
            bool success = false;
            GraphDatabaseHelpers.Instance.UnlockUser(userId);

            return Json(success);
        }

        public JsonResult LockPost(int postId, int reportID)
        {
            try
            {
                GraphDatabaseHelpers.Instance.LockPost(postId);
                GraphDatabaseHelpers.Instance.DeleteReportPost(reportID);
                return Json(true);
            }
            catch (Exception exception)
            {
                return Json(false);
            }
        }

        public JsonResult CancelReportPost(int reportID)
        {
            try
            {
                GraphDatabaseHelpers.Instance.DeleteReportPost(reportID);
                return Json(true);
            }
            catch (Exception exception)
            {
                return Json(false);
            }
        }

        public JsonResult CancelReportUser(int reportID)
        {
            try
            {
                GraphDatabaseHelpers.Instance.DeleteReportUser(reportID);
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
                List<Comment> listComment = GraphDatabaseHelpers.Instance.FindComment(post);
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