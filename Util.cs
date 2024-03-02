using System.Drawing;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Windows;

namespace ComputerGraphicsIProject
{
    public static class Util
    {
        public static void SaveBitmapToFile(Bitmap? bitmap, string fileName)
        {
            BitmapSource bitmapSource = Imaging.CreateBitmapSourceFromHBitmap(
                bitmap!.GetHbitmap(),
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());

            // Save BitmapSource to the specified file
            using (var fileStream = new System.IO.FileStream(fileName, System.IO.FileMode.Create))
            {
                BitmapEncoder? encoder = null;

                // Determine the file format based on the file extension
                string extension = System.IO.Path.GetExtension(fileName).ToLower();
                switch (extension)
                {
                    case ".png":
                        encoder = new PngBitmapEncoder();
                        break;
                    case ".jpg":
                    case ".jpeg":
                        encoder = new JpegBitmapEncoder();
                        break;
                    case ".bmp":
                        encoder = new BmpBitmapEncoder();
                        break;
                    default:
                        ShowMessageBoxError("Unsupported file format");
                        return;
                }

                encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
                encoder.Save(fileStream);
            }
        }

        public static void ShowMessageBoxError(string message)
        {
            MessageBox.Show(message);
        }
    }
}
