using System;
using System.Configuration;
using System.Net.Mail;
using System.Threading;
using System.Web.Mvc;
using Facebook;
using FlyAwayPlus.Helpers;
using FlyAwayPlus.Models;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v2;
using Google.Apis.Util.Store;
using ASPSnippets.GoogleAPI;
using System.Web.Script.Serialization;

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
            if (!ModelState.IsValid)
            {
                user.typeID = 0; // Fap Account

                // select from DB
                User newUser = GraphDatabaseHelpers.GetUser(user.typeID, user.email);

                /*
                 *  Insert into Graph DB 
                 */
                if (newUser == null)
                {
                    user.typeID = 0; // Fap account
                    user.dateJoined = DateTime.Now.ToString(FapConstants.DatetimeFormat);
                    //user.dateOfBirth = DateTime.Now.ToString();
                    user.status = "active";

                    // insert user to Database
                    GraphDatabaseHelpers.InsertUser(user);
                }
                else
                {
                    // TODO
                }

                // Set the auth cookie
                Session["authenicated"] = true;
                Session["username"] = user.firstName + " " + user.lastName;
                Session["userAva"] = user.avatar;
                UserHelpers.SetCurrentUser(Session, user);
            }

            //FormsAuthentication.SetAuthCookie(email, false);
            //SessionHelper.RenewCurrentUser();

            return RedirectToAction("Index", "Home");
        }

        public ActionResult Login(User user)
        {
            if (!ModelState.IsValid)
            {
                user.typeID = 0;

                var newUser = GraphDatabaseHelpers.GetUser(user.typeID, user.email);

                if (newUser == null)
                {
                    // TODO
                }
                else if (user.password.Equals(newUser.password))
                {
                    Session["authenicated"] = true;
                    Session["username"] = newUser.firstName + " " + newUser.lastName;
                    Session["userAva"] = user.avatar;
                    Session["user"] = newUser;
                }
            }

            return RedirectToAction("Index", "Home");
        }

        public ActionResult Logout()
        {
            Session["authenicated"] = "";
            Session["username"] = "";
            UserHelpers.SetCurrentUser(Session, null);
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
            User newUser = GraphDatabaseHelpers.GetUser(1, email); // Facebook account: typeID = 1

            /*
             *  Insert into Graph DB 
             */
            if (newUser == null)
            {
                newUser = new User
                {
                    typeID = 1,
                    email = email,
                    address = me.adress,
                    dateJoined = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"),
                    dateOfBirth = me.date_of_birth,
                    firstName = me.first_name,
                    lastName = me.last_name,
                    gender = me.gender,
                    phoneNumber = me.phone_number,
                    status = "active",
                    password = ""
                };
                // Facebook account

                // insert user to Database
                GraphDatabaseHelpers.InsertUser(newUser);
            }

            // Set the auth cookie
            Session["authenicated"] = true;
            Session["username"] = newUser.firstName + " " + newUser.lastName;
            Session["userAva"] = newUser.avatar;
            UserHelpers.SetCurrentUser(Session, newUser);

            //FormsAuthentication.SetAuthCookie(email, false);
            //SessionHelper.RenewCurrentUser();

            return RedirectToAction("Index", "Home");
        }

        private Uri RedirectUri
        {
            get
            {
                var uriBuilder = new UriBuilder(Request.Url)
                {
                    Query = null,
                    Fragment = null,
                    Path = Url.Action("FacebookCallback")
                };
                return uriBuilder.Uri;
            }
        }

        private Uri RedirectUriGoogle
        {
            get
            {
                var uriBuilder = new UriBuilder(Request.Url)
                {
                    Query = null,
                    Fragment = null,
                    Path = Url.Action("GoogleCallback")
                };
                return uriBuilder.Uri;
            }
        }

        public ActionResult ForgotPassword()
        {
            return View();
        }

        public ActionResult SendMail()
        {
            var x = Request["email"];
            string senderID = "flyawayplus.system@gmail.com";// use sender’s email id here..
            const string senderPassword = "doan2015"; // sender password here…
            try
            {
                var smtp = new SmtpClient
                {
                    Host = "smtp.gmail.com", // smtp server address here…
                    Port = 587,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    Credentials = new System.Net.NetworkCredential(senderID, senderPassword),
                    Timeout = 30000,
                };
                var message = new MailMessage(senderID, x, "subject", "body");
                smtp.Send(message);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

            }

            return RedirectToAction("Index", "Home");
        }

        public ActionResult AuthenGoogle()
        {
            GoogleConnect.ClientId = "929041403867-jj882kepf3cdpcm6dhr2r5qg6hsvrohf.apps.googleusercontent.com";
            GoogleConnect.ClientSecret = "2YuXPruTBTEkEHlkwXuZkvS2";
            GoogleConnect.RedirectUri = RedirectUriGoogle.AbsoluteUri.Split('?')[0];
            GoogleConnect.Authorize("profile", "email");
            return null;
        }

        public ActionResult GoogleCallback()
        {

            if (!string.IsNullOrEmpty(Request.QueryString["code"]))
            {
                string code = Request.QueryString["code"];
                string json = GoogleConnect.Fetch("me", code);
                User profile = new JavaScriptSerializer().Deserialize<User>(json);
                string email = profile.email;

                // select from DB
                User newUser = GraphDatabaseHelpers.GetUser(1, email); // Facebook account: typeID = 1

                /*
                 *  Insert into Graph DB 
                 */
                if (newUser == null)
                {
                    newUser = new User
                    {
                        typeID = 1,
                        email = email,
                        address = profile.address,
                        dateJoined = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"),
                        dateOfBirth = profile.dateOfBirth,
                        firstName = profile.firstName,
                        lastName = profile.lastName,
                        gender = profile.gender,
                        phoneNumber = profile.phoneNumber,
                        status = "active",
                        password = ""
                    };
                    // Facebook account

                    // insert user to Database
                    GraphDatabaseHelpers.InsertUser(newUser);
                }

                // Set the auth cookie
                Session["authenicated"] = true;
                Session["username"] = newUser.firstName + " " + newUser.lastName;
                Session["userAva"] = newUser.avatar;
                UserHelpers.SetCurrentUser(Session, newUser);

                //FormsAuthentication.SetAuthCookie(email, false);
                //SessionHelper.RenewCurrentUser();

                return RedirectToAction("Index", "Home");
            }

            return null;
        }
    }
}