using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace ComputerGraphicsIProject
{

    public static class ImageProcessor
    {
        public static Bitmap ApplyInversionFilter(Bitmap? original)
        {
            if (original == null)
            {
                throw new ArgumentNullException(nameof(original), "Input bitmap cannot be null.");
            }

            Bitmap result = new Bitmap(original.Width, original.Height);
 

            // Lock the bits of the original and result bitmaps
            BitmapData originalData = original.LockBits(new Rectangle(0, 0, original.Width, original.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            BitmapData resultData = result.LockBits(new Rectangle(0, 0, result.Width, result.Height), ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            try
            {
                unsafe
                {
                    // Pointer to the start of the pixel data
                    byte* originalPtr = (byte*)originalData.Scan0;
                    byte* resultPtr = (byte*)resultData.Scan0;

                    for (int y = 0; y < original.Height; y++)
                    {
                        for (int x = 0; x < original.Width; x++)
                        {
                            int offset = (y * originalData.Stride) + (x * 4); // Assuming 32bpp format (4 bytes per pixel)

                            // Access individual color channels
                            byte blue = originalPtr[offset];
                            byte green = originalPtr[offset + 1];
                            byte red = originalPtr[offset + 2];
                            byte alpha = originalPtr[offset + 3];

                            // Example: Invert the red channel
                            red = (byte)(255 - red);

                            // Set the result pixel
                            resultPtr[offset] = blue;
                            resultPtr[offset + 1] = green;
                            resultPtr[offset + 2] = red;
                            resultPtr[offset + 3] = alpha;
                        }
                    }
                }
            }
            finally
            {
                // Unlock the bits in a finally block to ensure it happens even if an exception occurs
                original.UnlockBits(originalData);
                result.UnlockBits(resultData);
            }

            return result;
        }
    }

}
