using System.Drawing.Imaging;
using System.Drawing;
using System.Windows;

namespace ComputerGraphicsIProject
{
    public static class ColorQuantization
    {
        public static void PopularityQuantization(Bitmap? bitmap, int numColors)
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

                    // Dictionary of colors
                    var colorHistogram = new Dictionary<Color, int>();

                    for (int y = 0; y < bitmap.Height; y++)
                    {
                        for (int x = 0; x < stride; x += 3)
                        {
                            byte blue = bitmapDataPtr[0];
                            byte green = bitmapDataPtr[1];
                            byte red = bitmapDataPtr[2];

                            Color color = Color.FromArgb(red, green, blue);

                            if (colorHistogram.ContainsKey(color))
                                colorHistogram[color]++;
                            else
                                colorHistogram[color] = 1;

                            bitmapDataPtr += 3; // Jump to the next pixel
                        }
                    }

                    var sortedColorHistogram = colorHistogram.OrderByDescending(pair => pair.Value);
                    var selectedPalette = sortedColorHistogram.Take(numColors).Select(pair => pair.Key).ToArray();

                    // Reset
                    bitmapDataPtr = (byte*)bitmapData.Scan0;

                    for (int y = 0; y < bitmap.Height; y++)
                    {
                        for (int x = 0; x < stride; x += 3)
                        {
                            byte blue = bitmapDataPtr[0];
                            byte green = bitmapDataPtr[1];
                            byte red = bitmapDataPtr[2];

                            int nearestColor = findNearestColor(red, green, blue, selectedPalette);

                            if (nearestColor != -1)
                            {
                                bitmapDataPtr[0] = selectedPalette[nearestColor].B;
                                bitmapDataPtr[1] = selectedPalette[nearestColor].G;
                                bitmapDataPtr[2] = selectedPalette[nearestColor].R;
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

        private static int findNearestColor(int r, int g, int b, Color[] palette)
        {
            double minDistance = double.MaxValue;
            int nearestIndex = 0;

            for (int i = 0; i < palette.Length; i++)
            {
                int dr = r - palette[i].R;
                int dg = g - palette[i].G;
                int db = b - palette[i].B;
                //MessageBox.Show(palette[i].ToString());

                double distance = Math.Sqrt(dr * dr + dg * dg + db * db);

                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestIndex = i;
                }
            }

            return nearestIndex;
        }
    }
}
