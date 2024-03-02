using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Documents;

namespace ComputerGraphicsIProject
{

    public static class FunctionalFilters
    {
        public static void ApplyFilter(Bitmap? inputBitmap, Func<byte, byte> FilterFunction)
        {
            /*
             * This implementation makes use these 2 resources to create an improved version
             * https://www.codeproject.com/Articles/1989/Image-Processing-for-Dummies-with-C-and-GDI-Part-1
             * https://learn.microsoft.com/en-us/dotnet/api/system.drawing.imaging.bitmapdata.scan0?view=dotnet-plat-ext-8.0#system-drawing-imaging-bitmapdata-scan0
             */
            if (inputBitmap == null)
                throw new ArgumentNullException("input");

            BitmapData bitmapData = inputBitmap.LockBits(new Rectangle(0, 0, inputBitmap.Width, 
                inputBitmap.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

            // Get the number of bytes in a stride
            int stride = bitmapData.Stride;

            try
            {
                unsafe
                {
                    
                    // Pointer to the first byte of the pixel data
                    byte* bitmapDataPtr = (byte*)bitmapData.Scan0;

                    for (int y = 0; y < inputBitmap.Height; ++y)
                    {
                        for (int x = 0; x < stride; x += 3)
                        {
                            bitmapDataPtr[0] = FilterFunction(bitmapDataPtr[0]); // Red channel
                            bitmapDataPtr[1] = FilterFunction(bitmapDataPtr[1]); // Green channel
                            bitmapDataPtr[2] = FilterFunction(bitmapDataPtr[2]); // Blue channel

                            bitmapDataPtr += 3; // Jump to the next pixel
                        }
                    }
                }
            }
            finally
            {
                inputBitmap.UnlockBits(bitmapData);
            }
        }

        public static byte Inversion(byte channelValue)
        {
            return (byte)(255 - channelValue);
        }

        public static byte BrightnessCorrection(byte channelValue, short adjustment)
        {
            int adjustedValue = channelValue + adjustment;
            return (byte)Math.Min(255, Math.Max(0, adjustedValue));
        }

    }
    
}
