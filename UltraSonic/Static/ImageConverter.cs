using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace UltraSonic.Static
{
    public static class ImageConverter
    {
        /// <summary>
        /// Converts a <see cref="System.Drawing.Image"/> into a WPF <see cref="BitmapSource"/>.
        /// </summary>
        /// <param name="source">The source image.</param>
        /// <returns>A BitmapSource</returns>
        public static BitmapSource ToBitmapSource(this Image source)
        {
            using (Bitmap bitmap = new Bitmap(source))
            {
                BitmapSource bitSrc = bitmap.ToBitmapSource();
                return bitSrc;
            }
        }

        /// <summary>
        /// Converts a <see cref="System.Drawing.Bitmap"/> into a WPF <see cref="BitmapSource"/>.
        /// </summary>
        /// <remarks>Uses GDI to do the conversion. Hence the call to the marshalled DeleteObject.
        /// </remarks>
        /// <param name="source">The source bitmap.</param>
        /// <returns>A BitmapSource</returns>
        private static BitmapSource ToBitmapSource(this Bitmap source)
        {
            BitmapSource bitSrc;

            var hBitmap = source.GetHbitmap();

            try
            {
                bitSrc = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            }
            catch (Win32Exception)
            {
                bitSrc = null;
            }
            finally
            {
                NativeMethods.DeleteObject(hBitmap);
            }

            return bitSrc;
        }

        public static BitmapFrame Resize(this BitmapSource photo, BitmapScalingMode scalingMode, bool preserveAspect, int width, int height)
        {
            DrawingGroup group = new DrawingGroup();

            double sourceHeight = photo.Height;
            double sourceWidth = photo.Width;
            double aspectRatio = sourceHeight/sourceWidth;

            double newWidth = 0;
            double newHeight = 0;

            if (preserveAspect)
            {
                if (width > 0)
                {
                    newWidth = width;
                    newHeight = Math.Round(width * aspectRatio, 0, MidpointRounding.AwayFromZero);
                }
                else if (height > 0)
                {
                    newWidth = Math.Round(height / aspectRatio, 0, MidpointRounding.AwayFromZero);
                    newHeight = height;
                }
            }
            else
            {
                newWidth = width;
                newHeight = height;
            }

            float dpiX;
            float dpiY;

            using (var graphics = Graphics.FromHwnd(IntPtr.Zero))
            {
                dpiX = graphics.DpiX;
                dpiY = graphics.DpiY;
            }

            RenderOptions.SetBitmapScalingMode(group, scalingMode);
            group.Children.Add(new ImageDrawing(photo, new Rect(0, 0, newWidth, newHeight)));
            DrawingVisual targetVisual = new DrawingVisual();
            DrawingContext targetContext = targetVisual.RenderOpen();
            targetContext.DrawDrawing(group);
            RenderTargetBitmap target = new RenderTargetBitmap((int)newWidth, (int)newHeight, dpiX, dpiY, PixelFormats.Default);
            targetContext.Close();
            target.Render(targetVisual);

            return BitmapFrame.Create(target);
        }
    }
}
