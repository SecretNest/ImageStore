using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecretNest.ImageStore.SimilarFile
{
    static class LoadImageHelper
    {
        internal static string cachePath;

        public static void ClearCache()
        {
            var files = Directory.GetFiles(cachePath);
            foreach (var file in files)
                System.IO.File.Delete(file);
        }

        public static void RemoveCache(Guid fileId)
        {
            if (cachePath != null)
            {
                var cacheFile = Path.Combine(cachePath, string.Format("{0:N}.png", fileId));
                if (System.IO.File.Exists(cacheFile))
                    System.IO.File.Delete(cacheFile);
            }
        }

        public static Image LoadFromFile(string path, out bool succeeded)
        {
            //return new Bitmap(640, 480);
            try
            {
                var result = Image.FromFile(path, true);
                result.Tag = path;
                succeeded = true;
                return result;
            }
            catch(FileNotFoundException)
            {
                succeeded = false;
                return DrawText(string.Format("File not found.\nFile: {0}", path),
                    SystemFonts.DefaultFont, SystemColors.ControlText, SystemColors.Window);
            }
            catch (Exception ex)
            {
                succeeded = false;
                return DrawText(string.Format("Exception occurred while open file.\nFile: {0}\nException: {1}", path, ex.Message),
                    SystemFonts.DefaultFont, SystemColors.ControlText, SystemColors.Window);
            }
        }

        static Image DrawText(string text, Font font, Color textColor, Color backColor)
        {
            //first, create a dummy bitmap just to get a graphics object
            Image img = new Bitmap(1, 1);
            Graphics drawing = Graphics.FromImage(img);

            //measure the string to see how big the image needs to be
            SizeF textSize = drawing.MeasureString(text, font);

            //free up the dummy image and old graphics object
            img.Dispose();
            drawing.Dispose();

            //create a new image of the right size
            img = new Bitmap((int)textSize.Width, (int)textSize.Height);

            drawing = Graphics.FromImage(img);

            //paint the background
            drawing.Clear(backColor);

            //create a brush for the text
            Brush textBrush = new SolidBrush(textColor);

            drawing.DrawString(text, font, textBrush, 0, 0);

            drawing.Save();

            textBrush.Dispose();
            drawing.Dispose();

            return img;

        }

        //public static void BuildCache(Guid fileId, string path)
        //{
        //    var cacheFile = Path.Combine(cachePath, string.Format("{0:N}.png", fileId));
        //    if (System.IO.File.Exists(cacheFile))
        //        return;

        //    using (var image = LoadFromFile(path, out bool succeeded))
        //    {
        //        var thumbprint = GenerateThumbprint(image);
        //        if (succeeded) thumbprint.Save(cacheFile);
        //        return;
        //    }
        //}

        public static Image GetThumbprintImage(Guid fileId, string path)
        {
            if (cachePath != null)
            {
                var cacheFile = Path.Combine(cachePath, string.Format("{0:N}.png", fileId));
                if (System.IO.File.Exists(cacheFile))
                    return Image.FromFile(cacheFile);

                using (var image = LoadFromFile(path, out bool succeeded))
                {
                    var thumbprint = GenerateThumbprint(image);
                    if (succeeded) thumbprint.Save(cacheFile);
                    return thumbprint;
                }
            }
            else
            {
                using (var image = LoadFromFile(path, out bool succeeded))
                {
                    var thumbprint = GenerateThumbprint(image);
                    return thumbprint;
                }
            }
        }

        const int thumbprintWidth = 64, thumbprintHeight = 64;

        static Image GenerateThumbprint(Image originalImage)
        {
            int sourceWidth = originalImage.Width;
            int sourceHeight = originalImage.Height;
            int sourceX = 0;
            int sourceY = 0;
            int destX = 0;
            int destY = 0;

            float nPercent;
            float nPercentW;
            float nPercentH;

            nPercentW = (thumbprintWidth / (float)sourceWidth);
            nPercentH = (thumbprintHeight / (float)sourceHeight);
            if (nPercentH < nPercentW)
            {
                nPercent = nPercentH;
                destX = Convert.ToInt16((thumbprintWidth -
                              (sourceWidth * nPercent)) / 2);
            }
            else
            {
                nPercent = nPercentW;
                destY = Convert.ToInt16((thumbprintHeight -
                              (sourceHeight * nPercent)) / 2);
            }

            int destWidth = (int)(sourceWidth * nPercent);
            int destHeight = (int)(sourceHeight * nPercent);

            Bitmap bmPhoto = new Bitmap(thumbprintWidth, thumbprintHeight,
                              PixelFormat.Format32bppRgb);
            bmPhoto.SetResolution(originalImage.HorizontalResolution,
                             originalImage.VerticalResolution);

            using (Graphics grPhoto = Graphics.FromImage(bmPhoto))
            {
                grPhoto.Clear(Color.White);
                grPhoto.InterpolationMode =
                        InterpolationMode.HighQualityBicubic;

                grPhoto.DrawImage(originalImage,
                    new Rectangle(destX, destY, destWidth, destHeight),
                    new Rectangle(sourceX, sourceY, sourceWidth, sourceHeight),
                    GraphicsUnit.Pixel);
            }
            return bmPhoto;
        }

        public static float GetResizePercent(Image original1, Image original2, Size size1, Size size2)
        {
            int source1Width = original1.Width;
            int source1Height = original1.Height;
            int source2Width = original2.Width;
            int source2Height = original2.Height;

            float nPercentW = Math.Min(size1.Width / (float)source1Width, size2.Width / (float)source2Width);
            float nPercentH = Math.Min(size1.Height / (float)source1Height, size2.Height / (float)source2Height);
            float nPercent;

            if (nPercentH < nPercentW)
            {
                nPercent = nPercentH;
            }
            else
            {
                nPercent = nPercentW;
            }

            return nPercent;
        }

        public static Image ResizeImage(Image original, Size size, float percent)
        {
            int sourceWidth = original.Width;
            int sourceHeight = original.Height;

            int destX = Convert.ToInt16((size.Width -
                  (sourceWidth * percent)) / 2);
            int destY = Convert.ToInt16((size.Height -
                  (sourceHeight * percent)) / 2);
            int destWidth = (int)(sourceWidth * percent);
            int destHeight = (int)(sourceHeight * percent);

            Bitmap resized = new Bitmap(size.Width, size.Height,
                  PixelFormat.Format32bppRgb);
            resized.SetResolution(original.HorizontalResolution,
                             original.VerticalResolution);
            using (Graphics grPhoto = Graphics.FromImage(resized))
            {
                grPhoto.Clear(Color.White);
                grPhoto.InterpolationMode =
                        InterpolationMode.HighQualityBicubic;

                grPhoto.DrawImage(original,
                    new Rectangle(destX, destY, destWidth, destHeight),
                    new Rectangle(0, 0, sourceWidth, sourceHeight),
                    GraphicsUnit.Pixel);
            }

            resized.Tag = original.Tag;
            return resized;
        }
    }
}
