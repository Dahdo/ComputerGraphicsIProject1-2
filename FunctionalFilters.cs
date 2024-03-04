using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Reflection.Metadata;
using System.Windows.Documents;

namespace ComputerGraphicsIProject
{

    public static class FunctionalFilters
    {
        public static void ApplyFilter(Bitmap? bitmap, Func<byte, byte> FilterFunction)
        {
            /*
             * This implementation makes use these 2 resources to create an improved version
             * https://www.codeproject.com/Articles/1989/Image-Processing-for-Dummies-with-C-and-GDI-Part-1
             * https://learn.microsoft.com/en-us/dotnet/api/system.drawing.imaging.bitmapdata.scan0?view=dotnet-plat-ext-8.0#system-drawing-imaging-bitmapdata-scan0
             */
            if (bitmap == null)
                throw new ArgumentNullException("input");

            BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width,
                bitmap.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

            // Get the number of bytes in a stride
            int stride = bitmapData.Stride;

            try
            {
                unsafe
                {

                    // Pointer to the first byte of the pixel data
                    byte* bitmapDataPtr = (byte*)bitmapData.Scan0;

                    for (int y = 0; y < bitmap.Height; y++)
                    {
                        for (int x = 0; x < stride; x += 3)
                        {
                            bitmapDataPtr[0] = FilterFunction(bitmapDataPtr[0]); // Blue channel
                            bitmapDataPtr[1] = FilterFunction(bitmapDataPtr[1]); // Green channel
                            bitmapDataPtr[2] = FilterFunction(bitmapDataPtr[2]); // Red channel

                            bitmapDataPtr += 3; // Jump to the next pixel
                        }
                    }
                }
            }
            finally
            {
                bitmap.UnlockBits(bitmapData);
            }
        }

        public static byte Inversion(byte channelValue)
        {
            return (byte)(255 - channelValue);
        }

        public static byte BrightnessCorrection(byte channelValue, short adjustment)
        {
            int channelValueAdjusted = channelValue + adjustment;
            return (byte)Math.Min(255, Math.Max(0, channelValueAdjusted));
        }

        public static byte ConstrastEnhancement(byte channelValue, float adjustment)
        {
            // Source: stackoverflow answer https://stackoverflow.com/a/3115178

            // Adjust the adjustment in range 0.0 - 0.04
            float adjustmentNormalized = (adjustment + 100.0f) / 100.0f;
            adjustmentNormalized *= adjustmentNormalized;

            // Normalize the channel value in range 0.0 - 1.0
            float channelValueNormalized = channelValue / 255.0f;
            // Apply the filter: Darken the values below 0.5 and lighten values above 0.5
            channelValueNormalized = (((channelValueNormalized - 0.5f) * adjustmentNormalized) + 0.5f) * 255.0f;
            return (byte)Math.Min(255, Math.Max(0, channelValueNormalized));
        }

        public static byte GammaCorrection(byte channelValue, float gamma, float c = 1.0f)
        {
            // Source: https://epochabuse.com/csharp-gamma-correction/

            return (byte)(c * Math.Pow(channelValue / 255.0f, gamma) * 255.0f);
        }
    }

}