using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Facebook;
using System.Configuration;
using Newtonsoft.Json;
using System.IO;
using FlyAwayPlus.Models;
using System.Web.Security;
using System.Net;
using Neo4jClient;
using FlyAwayPlus.Helpers;

namespace FlyAwayPlus.Controllers
{
    public class FapAccountController : Controller
    {
        private FlyAwayPlusEntities fap = new FlyAwayPlusEntities();

        //
        // GET: /FapAccount/
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Login()
        {
            string email = Request["username"];
            string password = Request["password"];

            List<User> listUser = null;

            User user = new User();
            user.email = email;
            user.typeID = 0;

            UserDetail userDetail = new UserDetail();
            
            listUser = fap.Users.Where(u => u.typeID == 0 && u.email.Equals(email)).ToList();
            if (listUser.Count == 0)
            {
                /*
                 * Doing something
                 */
            }
            else
            {
                user = listUser.First();
                userDetail.userID = user.userID;

                userDetail = fap.UserDetails.Find(userDetail.userID);
            }

            if (userDetail.password.Equals(password))
            {
                Session["authenicated"] = true;
                Session["username"] = userDetail.firstName + " " + userDetail.lastName;
                Session["user"] = user;
            }
            
            return RedirectToAction("Index", "Home");
        }


        public ActionResult Register()
        {
            string firstName = Request["first_name"];
            string lastName = Request["last_name"];
            string phoneNumber = Request["phone_number"];
            string dob = Request["dob"];
            string gender = Request["gender-options"];
            string address = Request["address"];
            string email = Request["email"];
            string password = Request["password"];
            string password_confirm = Request["password_confirmation"];

            User newUser = new User();
            newUser.typeID = 0; // Fap account
            newUser.email = email;

            UserDetail newUserDetail = new UserDetail();
            newUserDetail.address = address;
            newUserDetail.dateJoined = DateTime.Now;
            newUserDetail.dateOfBirth = DateTime.Now;
            newUserDetail.firstName = firstName;
            newUserDetail.lastName = lastName;
            newUserDetail.gender = gender;
            newUserDetail.phoneNumber = phoneNumber;
            newUserDetail.status = "active";
            newUserDetail.password = password;


            List<User> listUser = null;

            listUser = fap.Users.Where(user => user.typeID == 0 && user.email.Equals(email)).ToList();
            if (listUser.Count == 0)
            {
                fap.Users.Add(newUser);
                fap.SaveChanges();

                newUser = fap.Users.Where(user => user.typeID == 0 && user.email.Equals(email)).ToList().First();

                newUserDetail.userID = newUser.userID;

                fap.UserDetails.Add(newUserDetail);
                fap.SaveChanges();

                /*
                *  Insert into Graph DB 
                */
                GraphDatabaseHelpers.insertUser(newUser);
            }
            else
            {
                newUser = listUser.First();
            }


            // Set the auth cookie
            Session["authenicated"] = true;
            Session["username"] = newUserDetail.firstName + " " + newUserDetail.lastName;
            Session["user"] = newUser;

            //FormsAuthentication.SetAuthCookie(email, false);
            //SessionHelper.RenewCurrentUser();

            return RedirectToAction("Index", "Home");
        }

        /*
        [HttpPost]
        public ActionResult Register(HttpPostedFileBase file)
        {
            if (file.ContentLength > 0)
            {
                var fileName = Path.GetFileName(file.FileName);
                var path = Path.Combine(Server.MapPath("~/App_Data/"), fileName);
                file.SaveAs(path);
            }

            return RedirectToAction("Index");
        }
        */
        public ActionResult AuthenFacebook()
        {

            var fb = new FacebookClient();
            var loginUrl = fb.GetLoginUrl(new
            {
                client_id = ConfigurationManager.AppSettings["clientId"],
                //"974195839291185",
                client_secret = ConfigurationManager.AppSettings["clientSecret"],
                //"23aacf540a138ebcad87f1ee1825fc05",
                redirect_uri = RedirectUri.AbsoluteUri,
                response_type = "code",
                
                scope = "email" // Add other permissions as needed
            });


            return Redirect(loginUrl.AbsoluteUri);
        }

        public ActionResult FacebookCallback(string code)
        {
            var fb = new FacebookClient();
            dynamic result = fb.Post("oauth/access_token", new
            {
                client_id = ConfigurationManager.AppSettings["clientId"],
                client_secret = ConfigurationManager.AppSettings["clientSecret"],
                redirect_uri = RedirectUri.AbsoluteUri,
                code = code
            });

            var accessToken = result.access_token;

            // Store the access token in the session
            Session["AccessToken"] = accessToken;

            // update the facebook client with the access token so 
            // we can make requests on behalf of the user
            fb.AccessToken = accessToken;


            // Get the user's information
            dynamic me = fb.Get("/me");
            string email = me.email;

            User newUser = new User();
            newUser.typeID = 1; // Facebook account
            newUser.email = email;

            UserDetail newUserDetail = new UserDetail();
            newUserDetail.address = me.adress;
            newUserDetail.dateJoined = DateTime.Now;
            newUserDetail.dateOfBirth = me.date_of_birth;
            newUserDetail.firstName = me.first_name;
            newUserDetail.lastName = me.last_name;
            newUserDetail.gender = me.gender;
            newUserDetail.phoneNumber = me.phone_number;
            newUserDetail.status = "active";

            List<User> listUser = null;

            listUser = fap.Users.Where(user => user.typeID == 1 && user.email.Equals(email)).ToList();
            if (listUser.Count == 0)
            {
                fap.Users.Add(newUser);
                fap.SaveChanges();

                newUser = fap.Users.Where(user => user.typeID == 1 && user.email.Equals(email)).ToList().First();

                newUserDetail.userID = newUser.userID;

                fap.UserDetails.Add(newUserDetail);
                fap.SaveChanges();

                /*
                 *  Insert into Graph DB 
                 */
                GraphDatabaseHelpers.insertUser(newUser);
                
            }
            else
            {
                newUser = listUser.First();
            }

            // Set the auth cookie
            Session["authenicated"] = true;
            Session["username"] = newUserDetail.firstName + " " + newUserDetail.lastName;
            Session["user"] = newUser;

            //FormsAuthentication.SetAuthCookie(email, false);
            //SessionHelper.RenewCurrentUser();

            return RedirectToAction("Index", "Home");
        }

        private Uri RedirectUri
        {
            get
            {
                var uriBuilder = new UriBuilder(Request.Url);
                uriBuilder.Query = null;
                uriBuilder.Fragment = null;
                uriBuilder.Path = Url.Action("FacebookCallback");
                return uriBuilder.Uri;
            }
        }
    }
}