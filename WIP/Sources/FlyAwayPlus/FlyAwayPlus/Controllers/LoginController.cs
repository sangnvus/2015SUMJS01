using Facebook;
using FlyAwayPlus.Helpers;
using FlyAwayPlus.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FlyAwayPlus.Controllers
{
    public class LoginController : Controller
    {
        // GET: Login
        [ChildActionOnly]
        public ActionResult LoginPartial()
        {
            var model = new User();
            return PartialView("~/Views/Shared/LoginPartial.cshtml", model);
        }

        public ActionResult Register(User user)
        {
            if (ModelState.IsValid)
            {
                user.typeID = 0; // Fap Account

                // select from DB
                User newUser = GraphDatabaseHelpers.getUser(user.typeID, user.email);

                /*
                 *  Insert into Graph DB 
                 */
                if (newUser == null)
                {
                    user.typeID = 0; // Fap account
                    user.dateJoined = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                    //user.dateOfBirth = DateTime.Now.ToString();
                    user.status = "active";

                    // insert user to Database
                    GraphDatabaseHelpers.insertUser(user);
                }
                else
                {
                    // TODO
                }

                // Set the auth cookie
                Session["authenicated"] = true;
                Session["username"] = user.firstName + " " + user.lastName;
                Session["user"] = user;
            }

            //FormsAuthentication.SetAuthCookie(email, false);
            //SessionHelper.RenewCurrentUser();

            return RedirectToAction("Index", "Home");
        }

        public ActionResult Login(User user)
        {
            if (ModelState.IsValid)
            {
                user.typeID = 0;

                var newUser = GraphDatabaseHelpers.getUser(user.typeID, user.email);

                if (newUser == null)
                {
                    // TODO
                }
                else if (user.password.Equals(newUser.password))
                {
                    Session["authenicated"] = true;
                    Session["username"] = newUser.firstName + " " + newUser.lastName;
                    Session["user"] = newUser;
                }
            }

            return RedirectToAction("Index", "Home");
        }

        public ActionResult Logout()
        {
            Session["authenicated"] = "";
            Session["username"] = "";
            Session["user"] = "";
            return RedirectToAction("Index", "Home");
        }

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

            // select from DB
            User newUser = GraphDatabaseHelpers.getUser(1, email); // Facebook account: typeID = 1

            /*
             *  Insert into Graph DB 
             */
            if (newUser == null)
            {
                newUser = new User();
                newUser.typeID = 1; // Facebook account
                newUser.email = email;
                newUser.address = me.adress;
                newUser.dateJoined = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                newUser.dateOfBirth = me.date_of_birth;
                newUser.firstName = me.first_name;
                newUser.lastName = me.last_name;
                newUser.gender = me.gender;
                newUser.phoneNumber = me.phone_number;
                newUser.status = "active";
                newUser.password = "";

                // insert user to Database
                GraphDatabaseHelpers.insertUser(newUser);
            }

            // Set the auth cookie
            Session["authenicated"] = true;
            Session["username"] = newUser.firstName + " " + newUser.lastName;
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