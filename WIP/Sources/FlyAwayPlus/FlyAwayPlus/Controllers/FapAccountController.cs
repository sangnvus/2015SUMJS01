using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Facebook;
using System.Configuration;
using Newtonsoft.Json;
using System.IO;

namespace FlyAwayPlus.Controllers
{
    public class FapAccountController : Controller
    {
        //
        // GET: /FapAccount/
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Register()
        {
            return View();
        }

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
            dynamic me = fb.Get("me?fields=first_name,last_name,id,email");
            dynamic tem = fb.Get("/me");

            string email = me.email;
            
            // Set the auth cookie
            //FormsAuthentication.SetAuthCookie(email, false);

            return RedirectToAction("Register", "FapAccount");
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