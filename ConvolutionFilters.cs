using ComputerGraphicsIProject;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Media.Effects;


namespace ComputerGraphicsIProject
{

    // Source: https://softwarebydefault.com/2013/05/01/image-convolution-filters/


    public abstract class ConvolutionFilterBase
    {

        protected float[,] kernel = new float[,]
                     { { 1.0f, 1.0f, 1.0f, },
                        { 1.0f, 1.0f, 1.0f, },
                        {1.0f, 1.0f, 1.0f}, };
        protected float offset;
        protected float divisor;
        protected int anchorX;
        protected int anchorY;

        public virtual int SizeX
        {
            get => kernel.GetLength(1);
        }
        public virtual int SizeY
        {
            get => kernel.GetLength(0);
        }

        public virtual int AnchorX
        {
            get => anchorX;
            set => anchorX = value;
        }
        public virtual int AnchorY
        {
            get => anchorY;
            set => anchorY = value;
        }

        public virtual float Offset
        {
            get => offset;
            set => offset = value;
        }

        public virtual float Divisor
        {
            get => divisor;
            set => divisor = value;
        }

        public virtual float[,] Kernel
        {
            get => kernel;
            set
            {
                kernel = value;
                divisor = SizeX * SizeY;
                anchorX = SizeX / 2;
                anchorY = SizeY / 2;
            }
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
                        ApplyKernel(inputPtr, outputPtr, x, y, inputBitmapData.Stride, inputBitmapData.Height, ConvolutionalFilter);
                    }
                }

                // Unlock the bitmaps
                inputBitmap.UnlockBits(inputBitmapData);
                bitmap.UnlockBits(outputBitmapData);
            }
        }

        private static unsafe void ApplyKernel<T>(byte* inputPtr, byte* outputPtr, int x, int y, int stride, int height, T filter)
            where T: ConvolutionFilterBase
        {
            float sumReds = 0, sumGreens = 0, sumBlues = 0;

            for (int i = 0; i < filter.SizeY; i++)
            {
                for (int j = 0; j < filter.SizeX; j++)
                {
                    int offsetX = x + j - filter.AnchorX;
                    int offsetY = y + i - filter.AnchorY;

                    // Check if the current position is within the image bounds
                    if (offsetX >= 0 && offsetX < stride && offsetY >= 0 && offsetY < height)
                    {
                        // Get the color of the current pixel
                        byte* currentPixel = inputPtr + offsetY * stride + offsetX * 3;
                        sumReds += currentPixel[0] * filter.Kernel[i, j];
                        sumGreens += currentPixel[1] * filter.Kernel[i, j];
                        sumBlues += currentPixel[2] * filter.Kernel[i, j];
                    }
                }
            }

            // Update the output pixel
            byte* outputPixel = outputPtr + y * stride + x * 3;
            outputPixel[0] = (byte)Math.Min(255, Math.Max(0, sumReds / filter.Divisor));    // Blue channel
            outputPixel[1] = (byte)Math.Min(255, Math.Max(0, sumGreens / filter.Divisor));  // Green channel
            outputPixel[2] = (byte)Math.Min(255, Math.Max(0, sumBlues / filter.Divisor));  // Red channel
        }
    }
}


public class BlurFilter : ConvolutionFilterBase
{
    public BlurFilter()
    {
        divisor = this.SizeX * this.SizeY;
        anchorX = this.SizeX / 2;
        anchorY = this.SizeY / 2;
        offset = 1.0f;
    }
}