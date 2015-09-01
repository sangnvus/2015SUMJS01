using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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
            return RedirectToAction("Index", "Admin");

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

        public ActionResult Main(int? page, string tab, string searchString, string searchBy)
        {
            if (Session["administrator"] == null || Session["administrator"].Equals(""))
            {
                return RedirectToAction("Index", "Admin");
            }
            List<User> listAllUsers = GraphDatabaseHelpers.Instance.ListAllUsers();
            List<GraphDatabaseHelpers.ReportPost> listAllReportPost = GraphDatabaseHelpers.Instance.ListAllReportPosts();
            List<GraphDatabaseHelpers.ReportUser> listAllReportUser = GraphDatabaseHelpers.Instance.ListAllReportUsers();
            int numberOfUser = GraphDatabaseHelpers.Instance.NumberOfUser();
            int numberOfReportPost = GraphDatabaseHelpers.Instance.NumberOfReportPost();
            int numberOfReportUser = GraphDatabaseHelpers.Instance.NumberOfReportUser();

                
            IEnumerable<User> iPersonList = listAllUsers;
            if (!String.IsNullOrEmpty(searchString))
            {
                if (searchBy.Equals("userId"))
                {
                    iPersonList = iPersonList.Where(s => s.UserId.ToString(CultureInfo.InvariantCulture).Contains(searchString));
                }
                else if (searchBy.Equals("firstName"))
                {
                    iPersonList = iPersonList.Where(s => s.FirstName.Contains(searchString));
                }
                else if (searchBy.Equals("lastName"))
                {
                    iPersonList = iPersonList.Where(s => s.LastName.Contains(searchString));
                }
                else if (searchBy.Equals("email"))
                {
                    iPersonList = iPersonList.Where(s => s.Email.Contains(searchString));
                }

                ViewBag.SearchString = searchString;
            }

            const int pageSize = 3;
            int pageNumber = (page ?? 1);
            var onePageOfUsers = iPersonList.ToPagedList(pageNumber, pageSize);
            var onePageOfReportPosts = listAllReportPost.ToPagedList(pageNumber, pageSize);
            var onePageOfReportUsers = listAllReportUser.ToPagedList(pageNumber, pageSize);
            ViewBag.OnePageOfUsers = onePageOfUsers;
            ViewBag.OnePageOfReportPosts = onePageOfReportPosts;
            ViewBag.OnePageOfReportUsers = onePageOfReportUsers;
            ViewBag.Tab = tab;
            ViewBag.NumberOfUser = numberOfUser;
            ViewBag.NumberOfReportPost = numberOfReportPost;
            ViewBag.NumberOfReportUser = numberOfReportUser;
            ViewBag.SearchBy = searchBy;
            return View();
        }

        public JsonResult LockUser(int userId)
        {
            GraphDatabaseHelpers.Instance.LockUser(userId);
            return Json(true);
        }

        public JsonResult LockReportUser(int userReportedId)
        {
            GraphDatabaseHelpers.Instance.LockUser(userReportedId);
            GraphDatabaseHelpers.Instance.DeleteReportUser(userReportedId);
            return Json(true);
        }

        public JsonResult UnlockUser(int userId)
        {
            try
            {
                GraphDatabaseHelpers.Instance.UnlockUser(userId);
                return Json(true);
            }
            catch (Exception)
            {
                return Json(false);
            }
        }

        public JsonResult LockReportPost(int postId)
        {
            try
            {
                GraphDatabaseHelpers.Instance.LockPost(postId);
                GraphDatabaseHelpers.Instance.DeleteReportPost(postId);
                return Json(true);
            }
            catch (Exception)
            {
                return Json(false);
            }
        }

        public JsonResult CancelReportPost(int postId)
        {
            try
            {
                GraphDatabaseHelpers.Instance.DeleteReportPost(postId);
                return Json(true);
            }
            catch (Exception)
            {
                return Json(false);
            }
        }

        public JsonResult CancelReportUser(int userReportedId)
        {
            try
            {
                GraphDatabaseHelpers.Instance.DeleteReportUser(userReportedId);
                return Json(true);
            }
            catch (Exception)
            {
                return Json(false);
            }
        }

        public ActionResult ViewPost(int id)
        {
            try
            {
                Post post = GraphDatabaseHelpers.Instance.FindPostById(id);
                User userReported = GraphDatabaseHelpers.Instance.FindUser(post);

                List<Photo> photo = GraphDatabaseHelpers.Instance.FindPhoto(id);
                List<Comment> listComment = GraphDatabaseHelpers.Instance.FindComment(id);
                Video video = GraphDatabaseHelpers.Instance.FindVideo(id);

                ViewData["post"] = post;
                ViewData["user"] = userReported;
                ViewData["photo"] = photo;
                ViewData["listComment"] = listComment;
                ViewData["video"] = video;
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