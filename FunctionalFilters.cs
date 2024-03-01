using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Documents;

namespace ComputerGraphicsIProject
{

    public static class FunctionalFilters
    {
        public static void ApplyInversionFilter(Bitmap? inputBitmap)
        {
            if(inputBitmap == null)
                throw new ArgumentNullException("input");

            BitmapData bitmapData = inputBitmap.LockBits(new Rectangle(0, 0, inputBitmap.Width, 
                inputBitmap.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);


            int stride = bitmapData.Stride;
            int memOffset = stride - inputBitmap.Width * 3;
            int numBytesWidth = inputBitmap.Width * 3;
;

            try
            {
                unsafe
                {
                    // Pointer to the first byte of the pixel data
                    byte* bitmapDataPtr = (byte*)bitmapData.Scan0;

                    for (int y = 0; y < inputBitmap.Height; ++y)
                    {
                        for (int x = 0; x < numBytesWidth; x += 3)
                        {
                            bitmapDataPtr[0] = (byte)(255 - bitmapDataPtr[0]); 
                            bitmapDataPtr[1] = (byte)(255 - bitmapDataPtr[1]);
                            bitmapDataPtr[2] = (byte)(255 - bitmapDataPtr[2]);
                            bitmapDataPtr += 3;
                        }
                        bitmapDataPtr += memOffset;
                    }
                }
            }
            finally
            {
                inputBitmap.UnlockBits(bitmapData);
            }
            // inputBitmap.Save("C:\\Users\\Dahdo\\source\\repos\\ComputerGraphicsIProject\\test.png", ImageFormat.Png);
        }

    }



}
