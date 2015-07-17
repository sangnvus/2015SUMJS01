using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using ASPSnippets.GoogleAPI;
using Facebook;
using FlyAwayPlus.Helpers;
using FlyAwayPlus.Models;

namespace FlyAwayPlus.Controllers
{
    public class LoginController : Controller
    {
        // GET: Login
        [ChildActionOnly]
        public ActionResult LoginPartial()
        {
            var model = new User();
            return PartialView("~/Views/Shared/_LoginPartial.cshtml", model);
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UploadAvatar(IEnumerable<HttpPostedFileBase> files)
        {
            if (files == null || !files.Any())
            {
                var errorData = new
                {
                    success = false,
                    errorMessage = "No file uploaded."
                };
                return Json(errorData);
            }

            var file = files.FirstOrDefault();
            if (file == null || !IsImage(file))
            {
                var errorData = new
                {
                    success = false, 
                    errorMessage = "File is of wrong format."
                };
                return Json(errorData);
            }

            if (file.ContentLength <= 0)
            {
                var errorData = new
                {
                    success = false,
                    errorMessage = "File cannot be zero length."
                };
                return Json(errorData);
            }

            var webPath = GetTempSavedFilePath(file);
            var successData = new
            {
                success = true, 
                fileName = webPath.Replace("/", "\\")
            };
            return Json(successData);
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
                    Credentials = new NetworkCredential(senderID, senderPassword),
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

        ///////////////////////////
        // TINY HELPER FUNCTIONS //
        ///////////////////////////

        private const int AvatarStoredWidth = 100;  // Ava size to be saved
        private const int AvatarStoredHeight = 100; // Ava size to be saved
        private const int AvatarScreenWidth = 500;  // Size of image to be showed to resize, crop

        private const string TempFolder = "/Temp";
        private const string MapTempFolder = "~" + TempFolder;
        private const string AvatarPath = "\\Images\\UserUpload\\UserAvatar";

        private bool IsImage(HttpPostedFileBase file)
        {
            if (file == null) return false;
            
            return file.ContentType.Contains("image") ||
                FapConstants.ImageFileExtensions.Any(item => file.FileName.EndsWith(item, StringComparison.OrdinalIgnoreCase));
        }

        private string GetTempSavedFilePath(HttpPostedFileBase file, int counter = 0)
        {
            // Define destination
            var serverPath = HttpContext.Server.MapPath(TempFolder);
            if (Directory.Exists(serverPath) == false)
            {
                Directory.CreateDirectory(serverPath);
            }

            // Generate unique file name
            var fileName = Path.GetFileName(file.FileName);
            const string prepend = "item_";
            string finalFileName = prepend + ((counter)) + "_" + fileName;
            if (System.IO.File.Exists(Path.Combine(serverPath, finalFileName)))
            {
                return GetTempSavedFilePath(file, ++counter);
            }

            finalFileName = SaveTemporaryAvatarFileImage(file, serverPath, finalFileName);

            // Clean up old files after every save
            CleanUpTempFolder(1);
            return Path.Combine(TempFolder, finalFileName);
        }

        private static string SaveTemporaryAvatarFileImage(HttpPostedFileBase file, string serverPath, string fileName)
        {
            var img = new WebImage(file.InputStream);
            var ratio = img.Height / (double)img.Width;
            img.Resize(AvatarScreenWidth, (int)(AvatarScreenWidth * ratio));

            var fullFileName = Path.Combine(serverPath, fileName);
            if (System.IO.File.Exists(fullFileName))
            {
                System.IO.File.Delete(fullFileName);
            }

            img.Save(fullFileName);
            return Path.GetFileName(img.FileName);
        }

        private void CleanUpTempFolder(int hoursOld)
        {
            try
            {
                var currentUtcNow = DateTime.UtcNow;
                var serverPath = HttpContext.Server.MapPath("/Temp");
                if (!Directory.Exists(serverPath)) return;
                var fileEntries = Directory.GetFiles(serverPath);
                foreach (var fileEntry in fileEntries)
                {
                    var fileCreationTime = System.IO.File.GetCreationTimeUtc(fileEntry);
                    var res = currentUtcNow - fileCreationTime;
                    if (res.TotalHours > hoursOld)
                    {
                        System.IO.File.Delete(fileEntry);
                    }
                }
            }
            catch
            {
                // Deliberately empty.
            }
        }

        [HttpPost]
        public ActionResult SaveImage(string t, string l, string h, string w, string fileName)
        {
            try
            {
                // Calculate dimensions
                var top = Convert.ToInt32(t.Replace("-", "").Replace("px", ""));
                var left = Convert.ToInt32(l.Replace("-", "").Replace("px", ""));
                var height = Convert.ToInt32(h.Replace("-", "").Replace("px", ""));
                var width = Convert.ToInt32(w.Replace("-", "").Replace("px", ""));

                // Get file from temporary folder
                if (fileName != null)
                {
                    var fn = Path.Combine(Server.MapPath(MapTempFolder), Path.GetFileName(fileName));
                    // ...get image and resize it, ...
                    var img = new WebImage(fn);
                    img.Resize(width, height);
                    // ... crop the part the user selected, ...
                    img.Crop(top, left, height - top - AvatarStoredHeight, width - left - AvatarStoredWidth);
                    // ... delete the temporary file,...
                    System.IO.File.Delete(fn);
                    // ... and save the new one.
                    var newFileName = Path.Combine(AvatarPath, Path.GetFileName(fn));
                    var newFileLocation = HttpContext.Server.MapPath(newFileName);
                    var directoryName = Path.GetDirectoryName(newFileLocation);
                    if (directoryName != null && !Directory.Exists(directoryName))
                    {
                        Directory.CreateDirectory(directoryName);
                    }

                    img.Save(newFileLocation);
                    return Json(new
                                {
                                    success = true,
                                    avatarFileLocation = newFileName
                                });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, errorMessage = "Unable to upload file.\nERRORINFO: " + ex.Message });
            }
            return null;
        }
    }
}