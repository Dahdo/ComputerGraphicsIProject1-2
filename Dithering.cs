using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComputerGraphicsIProject
{
    public static class ErrorDiffusionDithering
    {
        public static void ApplyErrorDiffusion(Bitmap? bitmap)
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

                            bitmapDataPtr[0] = (byte)((bitmapDataPtr[0] < 128) ? 0 : 255); // Blue channel
                            bitmapDataPtr[1] = (byte)((bitmapDataPtr[1] < 128) ? 0 : 255); // Green channel
                            bitmapDataPtr[2] = (byte)((bitmapDataPtr[2] < 128) ? 0 : 255); // Red channel

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
