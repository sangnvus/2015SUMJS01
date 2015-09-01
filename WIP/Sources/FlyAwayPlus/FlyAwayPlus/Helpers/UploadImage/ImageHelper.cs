using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web;

namespace FlyAwayPlus.Helpers.UploadImage
{
    public class ImageHelper : SingletonBase<ImageHelper>
    {
        private ImageHelper() { }

        public bool ValidateExtension(string extension)
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
        public Image Scale(Image imgPhoto, int width, int height)
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
            if (destWidth <= width && destHeight <= height)
            {
                // No resizing is needed.
            }
            else if (width != 0 && height != 0)
            {
                if (destHeight / destWidth > (float)height / width)
                {
                    destWidth = height * sourceWidth / sourceHeight;
                    destHeight = height;
                }
                else
                {
                    destWidth = width;
                    destHeight = sourceHeight * width / sourceWidth;
                }
            }
            // change size proportially depending on width or height
            else if (height != 0)
            {
                destWidth = height * sourceWidth / sourceHeight;
                destHeight = height;
            }
            else
            {
                destWidth = width;
                destHeight = sourceHeight * width / sourceWidth;
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

        public Image ImageDataBase64ToImage(string imageData)
        {
            byte[] bytes = Convert.FromBase64String(imageData);

            Image image;
            using (var ms = new MemoryStream(bytes))
            {
                image = Image.FromStream(ms);
            }

            return image;
        }
    }
}