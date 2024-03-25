using System;
using System.Drawing.Imaging;
using System.Drawing;
using System.Windows.Media.Media3D;
using System.ComponentModel;

namespace ComputerGraphicsIProject
{
    public static class ErrorDiffusionDithering
    {
        public static void ApplyErrorDiffusion(Bitmap? bitmap, ErrorDistributionMatrixBase filter, byte numColorLevels)
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
                    byte* bitmapDataScan0 = (byte*)bitmapData.Scan0;
                    byte* bitmapDataPtr = bitmapDataScan0;

                    for (int y = 0; y < bitmap.Height; y++)
                    {
                        for (int x = 0; x < stride; x += 3)
                        {
                            byte[] pixelErrors = new byte[3]
                            {
                                bitmapDataPtr[0],
                                bitmapDataPtr[1],
                                bitmapDataPtr[2]
                            };

                            bitmapDataPtr[0] = approximateValue(bitmapDataPtr[0], numColorLevels); // Blue channel
                            bitmapDataPtr[1] = approximateValue(bitmapDataPtr[1], numColorLevels); // Green channel
                            bitmapDataPtr[2] = approximateValue(bitmapDataPtr[2], numColorLevels); // Red channel

                            // Calculate the errors
                            pixelErrors[0] -= bitmapDataPtr[0];
                            pixelErrors[1] -= bitmapDataPtr[1];
                            pixelErrors[2] -= bitmapDataPtr[2];

                            ApplyErrorDistribution(bitmapDataScan0, pixelErrors, x, y, stride, bitmap.Height, filter);

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
        

        private static unsafe void ApplyErrorDistribution<T>(byte* bitmapDataScan0, byte[] pixelErrors, int x, int y, int stride, int height, T filter)
            where T : ErrorDistributionMatrixBase
        {
            for (int i = filter.AnchorY; i < filter.SizeY; i++)
            {
                for (int j = 0; j < filter.SizeX; j++)
                {
                    int offsetX = x + j - filter.AnchorX;
                    int offsetY = y + i - filter.AnchorY;

                    // Get the color of the current pixel
                    if (offsetX < (stride / 3) && offsetX >= 0 && offsetY < height && offsetY >= 0)
                    {  
                        byte* currentPixel = bitmapDataScan0 + offsetY * stride + offsetX * 3;
                        currentPixel[0] = (byte)Math.Min(255, Math.Max(0, currentPixel[0] + (pixelErrors[0] * filter.Kernel[i, j]) / filter.Divisor));  // Blue channel
                        currentPixel[1] = (byte)Math.Min(255, Math.Max(0, currentPixel[1] + (pixelErrors[1] * filter.Kernel[i, j]) / filter.Divisor));  // Green channel
                        currentPixel[2] = (byte)Math.Min(255, Math.Max(0, currentPixel[2] + (pixelErrors[2] * filter.Kernel[i, j]) / filter.Divisor));  // Red channel
                    }
                }
            }
        }

        public abstract class ErrorDistributionMatrixBase : INotifyPropertyChanged
        {

            protected float[,]? kernel;
            protected int anchorX;
            protected int anchorY;
            protected string name;
            protected float divisor;

            public event PropertyChangedEventHandler? PropertyChanged;
            public virtual void OnPropertyChanged(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }

            public abstract string Name
            {
                get;
                set;
            }
            public virtual int SizeX
            {
                get => kernel!.GetLength(1);
                set
                {
                    Kernel = GenerateNeutralKernel(SizeY, value);
                }
            }
            private float[,] GenerateNeutralKernel(int _sizeY, int _sizeX)
            {
                float[,] newKernel = new float[_sizeY, _sizeX];
                for (int i = 0; i < newKernel.GetLength(0); i++)
                {
                    for (int j = 0; j < newKernel.GetLength(1); j++)
                    {
                        if (i == newKernel.GetLength(0) / 2 && j == newKernel.GetLength(1) / 2)
                            newKernel[i, j] = 1;
                        else
                            newKernel[i, j] = 0;
                    }
                }
                return newKernel;
            }
            public virtual int SizeY
            {
                get => kernel!.GetLength(0);
                set
                {
                    Kernel = GenerateNeutralKernel(value, SizeX);
                }
            }

            public virtual int AnchorX
            {
                get => anchorX;
                set
                {
                    anchorX = value;
                    OnPropertyChanged(nameof(AnchorX));
                }
            }
            public virtual int AnchorY
            {
                get => anchorY;
                set
                {
                    anchorY = value;
                    OnPropertyChanged(nameof(AnchorY));
                }
            }
            public virtual float[,] Kernel
            {
                get => kernel!;
                set
                {
                    kernel = value;
                    anchorX = SizeX / 2;
                    anchorY = SizeY / 2;
                    OnPropertyChanged(nameof(Kernel));
                }
            }

            public virtual float Divisor
            {
                get => divisor;
                set
                {
                    divisor = value;
                    OnPropertyChanged(nameof(Divisor));
                }
            }
        }

        public class FloydAndSteinbergFilter : ErrorDistributionMatrixBase
        {
            public FloydAndSteinbergFilter()
            {
                kernel = new float[,]
                {
                    {0, 0, 0 },
                    {0, 0, 7},
                    {3, 5, 1}
                };
                anchorX = this.SizeX / 2;
                anchorY = this.SizeY / 2;
                divisor = 16;
            }

            public override float[,] Kernel
            {
                get => kernel!;
                set
                {
                    kernel = value;
                    anchorX = SizeX / 2;
                    anchorY = SizeY / 2;
                    OnPropertyChanged(nameof(Kernel));
                }
            }
            public override string Name
            {
                get => "Floyd and Steinberg Filter";
                set
                {
                    name = value;
                }
            }
        }

        public class BurkesFilter : ErrorDistributionMatrixBase
        {
            public BurkesFilter()
            {
                kernel = new float[,]
                {
                    {0, 0, 0, 0, 0},
                    {0, 0, 0, 8, 4},
                    {2, 4, 8, 4, 2}
                };
                anchorX = this.SizeX / 2;
                anchorY = this.SizeY / 2;
                divisor = 32;
            }

            public override float[,] Kernel
            {
                get => kernel!;
                set
                {
                    kernel = value;
                    anchorX = SizeX / 2;
                    anchorY = SizeY / 2;
                    OnPropertyChanged(nameof(Kernel));
                }
            }
            public override string Name
            {
                get => "Burkes Filter";
                set
                {
                    name = value;
                }
            }
        }

        public class StuckyFilter : ErrorDistributionMatrixBase
        {
            public StuckyFilter()
            {
                kernel = new float[,]
                {
                    {0, 0, 0, 0, 0},
                    {0, 0, 0, 0, 0},
                    {0, 0, 0, 8, 4},
                    {2, 4, 8, 4, 2},
                    {1, 2, 4, 2, 1}
                };
                anchorX = this.SizeX / 2;
                anchorY = this.SizeY / 2;
                divisor = 42;
            }

            public override float[,] Kernel
            {
                get => kernel!;
                set
                {
                    kernel = value;
                    anchorX = SizeX / 2;
                    anchorY = SizeY / 2;
                    OnPropertyChanged(nameof(Kernel));
                }
            }
            public override string Name
            {
                get => "Stucky Filter";
                set
                {
                    name = value;
                }
            }
        }

        public class SierraFilter : ErrorDistributionMatrixBase
        {
            public SierraFilter()
            {
                kernel = new float[,]
                {
                    {0, 0, 0, 0, 0},
                    {0, 0, 0, 0, 0},
                    {0, 0, 0, 5, 3},
                    {2, 4, 5, 4, 2},
                    {0, 2, 3, 2, 0},
                };
                anchorX = this.SizeX / 2;
                anchorY = this.SizeY / 2;
                divisor = 32;
            }

            public override float[,] Kernel
            {
                get => kernel!;
                set
                {
                    kernel = value;
                    anchorX = SizeX / 2;
                    anchorY = SizeY / 2;
                    OnPropertyChanged(nameof(Kernel));
                }
            }
            public override string Name
            {
                get => "Sierra Filter";
                set
                {
                    name = value;
                }
            }
        }

        public class AtkinsonFilter : ErrorDistributionMatrixBase
        {
            public AtkinsonFilter()
            {
                kernel = new float[,]
                {
                    {0, 0, 0, 0, 0},
                    {0, 0, 0, 0, 0},
                    {0, 0, 0, 1, 1},
                    {0, 1, 1, 1, 0},
                    {0, 0, 1, 0, 0},
                };
                anchorX = this.SizeX / 2;
                anchorY = this.SizeY / 2;
                divisor = 8;
            }

            public override float[,] Kernel
            {
                get => kernel!;
                set
                {
                    kernel = value;
                    anchorX = SizeX / 2;
                    anchorY = SizeY / 2;
                    OnPropertyChanged(nameof(Kernel));
                }
            }
            public override string Name
            {
                get => "Atkinson Filter";
                set
                {
                    name = value;
                }
            }
        }
    }
}
