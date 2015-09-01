using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Web;

namespace FlyAwayPlus.Helpers.UploadImage
{
    public class ImageUpload
    {
        // Set default size here
        public int Width { get; set; }

        public int Height { get; set; }

        // Folder for the upload, you can put this in the web.config
        private readonly string _uploadPath = "~/Images/UserUpload/";

        public ImageResult RenameUploadFile(HttpPostedFileBase file, int counter = 0)
        {
            var fileName = Path.GetFileName(file.FileName);

            const string prepend = "item_";
            var finalFileName = prepend + ((counter)) + "_" + fileName;
            return File.Exists(HttpContext.Current.Request.MapPath(_uploadPath + finalFileName)) 
                ? 
                RenameUploadFile(file, ++counter) 
                : 
                UploadFile(file, finalFileName);
        }

        private ImageResult UploadFile(HttpPostedFileBase file, string fileName)
        {
            ImageResult imageResult = new ImageResult { Success = true, ErrorMessage = null };

            var path =
          Path.Combine(HttpContext.Current.Request.MapPath(_uploadPath), fileName);
            string extension = Path.GetExtension(file.FileName);

            //make sure the file is valid
            if (!ImageHelper.Instance.ValidateExtension(extension))
            {
                imageResult.Success = false;
                imageResult.ErrorMessage = "Invalid Extension";
                return imageResult;
            }

            try
            {
                file.SaveAs(path);

                Image imgOriginal = Image.FromFile(path);

                Image imgActual = ImageHelper.Instance.Scale(imgOriginal, Width, Height);
                imgOriginal.Dispose();
                imgActual.Save(path, ImageFormat.Jpeg);
                imgActual.Dispose();

                imageResult.ImageName = fileName;

                return imageResult;
            }
            catch (Exception ex)
            {
                imageResult.Success = false;
                imageResult.ErrorMessage = ex.Message;
                return imageResult;
            }
        }
    }
}