using System;
using System.Drawing.Imaging;
using System.Drawing;

namespace ComputerGraphicsIProject
{
    public static class ErrorDiffusionDithering
    {
        public static void ApplyErrorDiffusion(Bitmap? bitmap, byte numColorLevels)
        {
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

                            bitmapDataPtr[0] = approximateValue(bitmapDataPtr[0], numColorLevels); // Blue channel
                            bitmapDataPtr[1] = approximateValue(bitmapDataPtr[1], numColorLevels); // Green channel
                            bitmapDataPtr[2] = approximateValue(bitmapDataPtr[2], numColorLevels); // Red channel

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

        private static byte approximateValue(byte channelValue, byte numColorLevels)
        {
            byte[] colorLevels = generateColorLevels(numColorLevels);
            byte closestLevel = colorLevels[0];
            int minDifference = Math.Abs(channelValue - closestLevel);

            foreach (byte level in colorLevels)
            {
                int difference = Math.Abs(channelValue - level);
                if (difference < minDifference)
                {
                    minDifference = difference;
                    closestLevel = level;
                }
            }

            return closestLevel;
        }

        private static byte[] generateColorLevels(byte numColorLevels)
        {
            byte[] colorLevels = new byte[numColorLevels];
            float step = 255 / (numColorLevels - 1);

            for (int i = 0; i < numColorLevels; i++)
            {
                colorLevels[i] = (byte)(i * step);
            }

            return colorLevels;
        }
        

        private static unsafe void ApplyKernel<T>(byte* inputPtr, byte* outputPtr, int x, int y, int stride, int height, T filter)
            where T : ConvolutionFilterBase
        {
            float sumReds = 0, sumGreens = 0, sumBlues = 0;

            for (int i = 0; i < filter.SizeY; i++)
            {
                for (int j = 0; j < filter.SizeX; j++)
                {
                    // Wrap around in case the offset extends beyond the raster
                    int offsetX = (x + j - filter.AnchorX + (stride / 3)) % (stride / 3);
                    int offsetY = (y + i - filter.AnchorY + height) % height;

                    // Get the color of the current pixel
                    byte* currentPixel = inputPtr + offsetY * stride + offsetX * 3;
                    sumBlues += currentPixel[0] * filter.Kernel[i, j];
                    sumGreens += currentPixel[1] * filter.Kernel[i, j];
                    sumReds += currentPixel[2] * filter.Kernel[i, j];
                }
            }

            // Write values to the current pixel
            outputPtr[0] = (byte)Math.Min(255, Math.Max(0, filter.Offset + sumBlues / filter.Divisor));    // Blue channel
            outputPtr[1] = (byte)Math.Min(255, Math.Max(0, filter.Offset + sumGreens / filter.Divisor));  // Green channel
            outputPtr[2] = (byte)Math.Min(255, Math.Max(0, filter.Offset + sumReds / filter.Divisor));  // Red channel
        }
    }
}
