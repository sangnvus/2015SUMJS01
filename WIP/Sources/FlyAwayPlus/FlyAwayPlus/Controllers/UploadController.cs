using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using FlyAwayPlus.Helpers;
using FlyAwayPlus.Helpers.UploadImage;

namespace FlyAwayPlus.Controllers
{
    public class UploadController : Controller
    {
        [HttpPost]
        public async Task<JsonResult> UploadPhoto()
        {
            var uploadStatuses = new List<string>();
            try
            {
                uploadStatuses.AddRange(from string file in Request.Files select UploadSinglePhoto(Request.Files[file]));
            }
            catch (Exception)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json("Upload failed");
            }

            return Json(uploadStatuses);
        }

        private string UploadSinglePhoto(HttpPostedFileBase file)
        {
            if (file.ContentLength <= 0)
                return "#fail_" + file.FileName;

            var imageUpload = new ImageUpload
            {
                Width = FapConstants.UploadedImageMaxWidthPixcel,
                Height = FapConstants.UploadedImageMaxHeightPixcel
            };

            var imageResult = imageUpload.RenameUploadFile(file);

            if (imageResult.Success)
            {
                return imageResult.ImageName;
            }

            return "#fail_" + imageResult.ImageName;
        }
    }
}