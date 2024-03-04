using ComputerGraphicsIProject;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;


namespace ComputerGraphicsIProject
{

    // Source: https://softwarebydefault.com/2013/05/01/image-convolution-filters/


    public abstract class ConvolutionFilterBase
    {
        public abstract float Offset
        {
            get;
        }


        public abstract float Divisor
        {
            get;
        }


        public abstract float[,] Kernel
        {
            get;
        }
    }

    public static class ConvolutionFilters
    {// my proposals: passing the kernel(we will be able to get size here) and anchor position (using variables like k)
        public static void ApplyFilter<T>(Bitmap? bitmap, T ConvolutionalFilter)
            where T : ConvolutionFilterBase
        {
            if (bitmap == null)
                throw new ArgumentNullException("input");
            // Create a copy of a bitmap and inputBitmapData in readonly mode
            Bitmap? inputBitmap = bitmap.Clone() as Bitmap;
            BitmapData inputBitmapData = inputBitmap!.LockBits(new Rectangle(0, 0, inputBitmap!.Width,
                inputBitmap.Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

            // Create outputBitmapData in writeonly mode
            BitmapData outputBitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width,
                bitmap.Height), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

            unsafe
            {
                byte* inputPtr = (byte*)inputBitmapData.Scan0;
                byte* outputPtr = (byte*)outputBitmapData.Scan0;

                for (int y = 0; y < inputBitmapData.Height; y++)
                {
                    for (int x = 0; x < inputBitmapData.Width; x++)
                    {
                        // Apply the convolution filter to the pixel
                        ApplyKernel(inputPtr, outputPtr, x, y, inputBitmapData.Stride, inputBitmapData.Width, inputBitmapData.Height, ConvolutionalFilter.Kernel);
                    }
                }

                // Unlock the bitmaps
                inputBitmap.UnlockBits(inputBitmapData);
                bitmap.UnlockBits(outputBitmapData);
            }
        }

        private static unsafe void ApplyKernel(byte* inputPtr, byte* outputPtr, int x, int y, int stride, int width, int height, float[,] kernel)
        {
            float sumReds = 0, sumGreens = 0, sumBlues = 0;

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    int offsetX = x + j - 1;
                    int offsetY = y + i - 1;

                    // Check if the current position is within the image bounds
                    if (offsetX >= 0 && offsetX < width && offsetY >= 0 && offsetY < height)
                    {
                        // Get the color of the current pixel
                        byte* currentPixel = inputPtr + offsetY * stride + offsetX * 3;
                        sumReds += currentPixel[2] * kernel[i, j];
                        sumGreens += currentPixel[1] * kernel[i, j];
                        sumBlues += currentPixel[0] * kernel[i, j];
                    }
                }
            }

            // Update the output pixel
            byte* outputPixel = outputPtr + y * stride + x * 3;
            outputPixel[2] = (byte)Math.Min(255, Math.Max(0, sumReds / 9)); // Red channel
            outputPixel[1] = (byte)Math.Min(255, Math.Max(0, sumGreens / 9)); // Green channel
            outputPixel[0] = (byte)Math.Min(255, Math.Max(0, sumBlues / 9)); // Blue channel
            outputPixel[3] = 255; // Alpha channel
        }
    }
}


public class BlurFilter : ConvolutionFilterBase
{
    private float[,] kernel = new float[,]
                     { { 1.0f, 1.0f, 1.0f, },
                        { 1.0f, 1.0f, 1.0f, },
                        {1.0f, 1.0f, 1.0f}, };

    public override float Offset => throw new NotImplementedException();

    public override float Divisor => throw new NotImplementedException();


    public override float[,] Kernel
    {
        get { return kernel; }
    }
}