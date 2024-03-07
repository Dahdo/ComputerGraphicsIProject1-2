using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Reflection.Metadata;
using System.Windows.Documents;

namespace ComputerGraphicsIProject
{

    public static class FunctionalFilters
    {
        public static void ApplyFilter(Bitmap? bitmap, Func<byte, byte> FilterFunction, int selectedChannel)
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
                            if(selectedChannel == -1)
                            {
                                bitmapDataPtr[0] = FilterFunction(bitmapDataPtr[0]); // Blue channel
                                bitmapDataPtr[1] = FilterFunction(bitmapDataPtr[1]); // Green channel
                                bitmapDataPtr[2] = FilterFunction(bitmapDataPtr[2]); // Red channel
                            }
                            else
                            {
                                bitmapDataPtr[selectedChannel] = FilterFunction(bitmapDataPtr[selectedChannel]); // Any selected channel
                            }
                            

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





        public static void ApplyFilterLabToHSV(Bitmap? bitmap)
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
                            //bitmapDataPtr[0] = FilterFunction(bitmapDataPtr[0]); // Blue channel
                            //bitmapDataPtr[1] = FilterFunction(bitmapDataPtr[1]); // Green channel
                            //bitmapDataPtr[2] = FilterFunction(bitmapDataPtr[2]); // Red channel

                            byte[] hsv = GetHSV(bitmapDataPtr[2], bitmapDataPtr[1], bitmapDataPtr[0]);

                            bitmapDataPtr[2] = hsv[0];
                            bitmapDataPtr[1] = hsv[1];
                            bitmapDataPtr[0] = hsv[2];


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

        public static void ApplyFilterLabToRGB(Bitmap? bitmap)
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
                            //bitmapDataPtr[0] = FilterFunction(bitmapDataPtr[0]); // Blue channel
                            //bitmapDataPtr[1] = FilterFunction(bitmapDataPtr[1]); // Green channel
                            //bitmapDataPtr[2] = FilterFunction(bitmapDataPtr[2]); // Red channel

                            byte[] rgb = GetRGBFromHSV(bitmapDataPtr[2], bitmapDataPtr[1], bitmapDataPtr[0]);

                            bitmapDataPtr[2] = rgb[0];
                            bitmapDataPtr[1] = rgb[1];
                            bitmapDataPtr[0] = rgb[2];


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

        static byte[] GetHSV(byte red, byte green, byte blue)
        {
            // Create System.Drawing.Color object
            Color color = Color.FromArgb(red, green, blue);

            // Calculate HSV values
            float hue = color.GetHue();
            float saturation = color.GetSaturation();
            float value = color.GetBrightness();

            // Convert HSV values to byte range (0-255)
            byte clampedHue = (byte)(hue / 360 * 255);
            byte clampedSaturation = (byte)(saturation * 255);
            byte clampedValue = (byte)(value * 255);

            // Return clamped HSV values in a byte array
            return new byte[] { clampedHue, clampedSaturation, clampedValue };
        }

        static byte[] GetRGBFromHSV(byte hue, byte saturation, byte value)
        {
            // Convert HSV values from byte range (0-255) to float range (0-1)
            float floatHue = hue * 360f / 255f;
            float floatSaturation = saturation / 255f;
            float floatValue = value / 255f;

            // Create System.Drawing.Color object
            Color color = ColorFromHSV(floatHue, floatSaturation, floatValue);

            // Extract RGB values
            byte red = color.R;
            byte green = color.G;
            byte blue = color.B;

            // Return RGB values in a byte array
            return new byte[] { red, green, blue };
        }

        static Color ColorFromHSV(float hue, float saturation, float value)
        {
            int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
            float f = hue / 60 - (float)Math.Floor(hue / 60);

            value = value * 255;
            int v = Convert.ToInt32(value);
            int p = Convert.ToInt32(value * (1 - saturation));
            int q = Convert.ToInt32(value * (1 - f * saturation));
            int t = Convert.ToInt32(value * (1 - (1 - f) * saturation));

            switch (hi)
            {
                case 0:
                    return Color.FromArgb(255, v, t, p);
                case 1:
                    return Color.FromArgb(255, q, v, p);
                case 2:
                    return Color.FromArgb(255, p, v, t);
                case 3:
                    return Color.FromArgb(255, p, q, v);
                case 4:
                    return Color.FromArgb(255, t, p, v);
                default:
                    return Color.FromArgb(255, v, p, q);
            }
        }
}

    

}