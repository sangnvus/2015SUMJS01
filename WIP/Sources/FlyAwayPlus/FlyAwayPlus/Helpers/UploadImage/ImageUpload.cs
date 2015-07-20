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
        private readonly string UploadPath = "~/Images/UserUpload/";

        public ImageResult RenameUploadFile(HttpPostedFileBase file, int counter = 0)
        {
            var fileName = Path.GetFileName(file.FileName);

            const string prepend = "item_";
            var finalFileName = prepend + ((counter)) + "_" + fileName;
            return File.Exists(HttpContext.Current.Request.MapPath(UploadPath + finalFileName)) 
                ? 
                RenameUploadFile(file, ++counter) 
                : 
                UploadFile(file, finalFileName);
        }

        private ImageResult UploadFile(HttpPostedFileBase file, string fileName)
        {
            ImageResult imageResult = new ImageResult { Success = true, ErrorMessage = null };

            var path =
          Path.Combine(HttpContext.Current.Request.MapPath(UploadPath), fileName);
            string extension = Path.GetExtension(file.FileName);

            //make sure the file is valid
            if (!ValidateExtension(extension))
            {
                imageResult.Success = false;
                imageResult.ErrorMessage = "Invalid Extension";
                return imageResult;
            }

            try
            {
                file.SaveAs(path);

                Image imgOriginal = Image.FromFile(path);

                //pass in whatever value you want 
                Image imgActual = Scale(imgOriginal);
                imgOriginal.Dispose();
                imgActual.Save(path);
                imgActual.Dispose();

                imageResult.ImageName = fileName;

                return imageResult;
            }
            catch (Exception ex)
            {
                // you might NOT want to show the exception error for the user
                // this is generaly logging or testing

                imageResult.Success = false;
                imageResult.ErrorMessage = ex.Message;
                return imageResult;
            }
        }

        private bool ValidateExtension(string extension)
        {
            extension = extension.ToLower();
            switch (extension)
            {
                case ".jpg":
                    return true;
                case ".png":
                    return true;
                case ".gif":
                    return true;
                case ".jpeg":
                    return true;
                default:
                    return false;
            }
        }

        private Image Scale(Image imgPhoto)
        {
            float sourceWidth = imgPhoto.Width;
            float sourceHeight = imgPhoto.Height;

            float destWidth = sourceWidth;
            float destHeight = sourceHeight;
            int sourceX = 0;
            int sourceY = 0;
            int destX = 0;
            int destY = 0;

            // force resize, might distort image
            if (destWidth <= Width && destHeight <= Height)
            {
                // No resizing is needed.
            }
            else if (Width != 0 && Height != 0)
            {
                if (destHeight / destWidth > (float)Height / Width)
                {
                    destWidth = Height * sourceWidth / sourceHeight;
                    destHeight = Height;
                }
                else
                {
                    destWidth = Width;
                    destHeight = sourceHeight * Width / sourceWidth;
                }
            }
            // change size proportially depending on width or height
            else if (Height != 0)
            {
                destWidth = Height * sourceWidth / sourceHeight;
                destHeight = Height;
            }
            else
            {
                destWidth = Width;
                destHeight = sourceHeight * Width / sourceWidth;
            }

            Bitmap bmPhoto = new Bitmap((int)destWidth, (int)destHeight,
                                        PixelFormat.Format32bppPArgb);
            bmPhoto.SetResolution(imgPhoto.HorizontalResolution, imgPhoto.VerticalResolution);

            Graphics grPhoto = Graphics.FromImage(bmPhoto);
            grPhoto.InterpolationMode = InterpolationMode.HighQualityBicubic;

            grPhoto.DrawImage(imgPhoto,
                new Rectangle(destX, destY, (int)destWidth, (int)destHeight),
                new Rectangle(sourceX, sourceY, (int)sourceWidth, (int)sourceHeight),
                GraphicsUnit.Pixel);

            grPhoto.Dispose();

            return bmPhoto;
        }
    }
}