﻿using ComputerGraphicsIProject;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using System.Windows;
using System.Linq;
using System.ComponentModel;


namespace ComputerGraphicsIProject
{

    // Source: https://softwarebydefault.com/2013/05/01/image-convolution-filters/


    public abstract class ConvolutionFilterBase : INotifyPropertyChanged
    {

        protected float[,]? kernel;
        protected float offset;
        protected float divisor;
        protected int anchorX;
        protected int anchorY;
        protected float sigma;
        protected string name;

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

        public virtual float Offset
        {
            get => offset;
            set
            {
                offset = value;
                OnPropertyChanged(nameof(Offset));
            }
        }

        public virtual float Divisor
        {
            get => divisor;
            set {
                divisor = value;
                OnPropertyChanged(nameof(Divisor));
            }
        }

        public virtual float[,] Kernel
        {
            get => kernel!;
            set
            {
                kernel = value;
                divisor = SizeX * SizeY;
                anchorX = SizeX / 2;
                anchorY = SizeY / 2;
                OnPropertyChanged(nameof(Kernel));
            }
        }

        public virtual float Sigma
        {
            set => sigma = value;
        }

        public void CalculateDivisor()
        {
            Divisor = SizeX * SizeY;
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
    }

    public static class ConvolutionFilters
    {
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

                for (int y = 0; y < inputBitmap.Height; y++)
                {
                    for (int x = 0; x < inputBitmapData.Stride / 3; x++)
                    {
                        // Apply the convolution filter to the pixel
                        ApplyKernel(inputPtr, outputPtr, x, y, inputBitmapData.Stride, inputBitmapData.Height, ConvolutionalFilter);

                        outputPtr += 3; // Jump to the next pixel
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


    public class BlurFilter : ConvolutionFilterBase
    {
        public BlurFilter()
        {
            kernel = new float[,]
                         {{1, 1, 1},
                           {1, 1, 1},
                           {1, 1, 1}};

            divisor = this.SizeX * this.SizeY;
            anchorX = this.SizeX / 2;
            anchorY = this.SizeY / 2;
            offset = 0.0f;
        }

        public override string Name
        {
            get => "Blur";
            set
            {
                name = value;
            }
        }
    }

    public class GaussianBlurFilter : ConvolutionFilterBase
    {
        // Source: https://hackernoon.com/how-to-implement-gaussian-blur-zw28312m
        private int size;
        public GaussianBlurFilter(int _size = 3, float _sigma = 1.5f)
        {
            this.size = _size;
            sigma = _sigma;
            this.GenerateKernel();
            divisor = 1;
            anchorX = this.SizeX / 2;
            anchorY = this.SizeY / 2;
            offset = 0.0f;
        }
        private void GenerateKernel()
        {
            kernel = new float[size, size];
            float sum = 0.0f;
            int halfSize = size / 2;

            for (int y = -halfSize; y <= halfSize; y++)
            {
                for (int x = -halfSize; x <= halfSize; x++)
                {
                    float exponent = -(x * x + y * y) / (2.0f * sigma * sigma);
                    kernel[y + halfSize, x + halfSize] = (float)(Math.Exp(exponent) / (2.0f * Math.PI * sigma * sigma));
                    sum += kernel[y + halfSize, x + halfSize];
                }
            }

            // Normalize Kernel
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    kernel[i, j] /= sum;
                }
            }

        }

        public override string Name
        {
            get => "Gaussian Blur";
            set
            {
                name = value;
            }
        }

        public override float[,] Kernel
        {
            get => kernel!;
            set
            {
                kernel = value;
                divisor = 1;
                anchorX = SizeX / 2;
                anchorY = SizeY / 2;
                OnPropertyChanged(nameof(Kernel));
            }
        }
    }


    public class SharpenFilter : ConvolutionFilterBase
    {
        public SharpenFilter()
        {
            kernel = new float[,]
            {
                {0, -1, 0 },
                {-1, 5, -1},
                {0, -1, 0 }
            };
            divisor = 1;
            anchorX = this.SizeX / 2;
            anchorY = this.SizeY / 2;
            offset = 0.0f;
        }

        public override float[,] Kernel
        {
            get => kernel!;
            set
            {
                kernel = value;
                divisor = 1;
                anchorX = SizeX / 2;
                anchorY = SizeY / 2;
                OnPropertyChanged(nameof(Kernel));
            }
        }
        public override string Name
        {
            get => "Sharpen";
            set
            {
                name = value;
            }
        }
    }

    public class EdgeDetectionFilter : ConvolutionFilterBase
    {
        private int size;
        public EdgeDetectionFilter()
        {
            kernel = new float[,]
            {
                { -1,  -1, -1 },
                { -1,  8, -1},
                { -1,  -1, -1 }
            };
            divisor = 1;
            anchorX = this.SizeX / 2;
            anchorY = this.SizeY / 2;
            offset = 0.0f;
        }
        public override float[,] Kernel
        {
            get => kernel!;
            set
            {
                kernel = value;
                divisor = 1;
                anchorX = SizeX / 2;
                anchorY = SizeY / 2;
                OnPropertyChanged(nameof(Kernel));
            }
        }
        public override string Name
        {
            get => "Edge Detection";
            set
            {
                name = value;
            }
        }
    }

    public class EmbossFilter : ConvolutionFilterBase
    {
        private int size;
        public EmbossFilter()
        {
            kernel = new float[,]
            {
                {-1, -1, 0},
                {-1, 1, 1},
                {0, 1, 1 }
            };
            divisor = 1;
            anchorX = this.SizeX / 2;
            anchorY = this.SizeY / 2;
            offset = 0.0f;
        }
        public override float[,] Kernel
        {
            get => kernel!;
            set
            {
                kernel = value;
                divisor = 1;
                anchorX = SizeX / 2;
                anchorY = SizeY / 2;
                OnPropertyChanged(nameof(Kernel));
            }
        }
        public override string Name
        {
            get => "Emboss";
            set
            {
                name = value;
            }
        }
    }

    public class GenericFilter : ConvolutionFilterBase
    {
        private int size;
        public GenericFilter()
        {
            kernel = new float[,]
            {
                { 0, 0, 0},
                { 0, 1, 0},
                { 0, 0, 0}
            };
            divisor = 1;
            anchorX = this.SizeX / 2;
            anchorY = this.SizeY / 2;
            offset = 0.0f;
        }

        public override float[,] Kernel
        {
            get => kernel!;
            set
            {
                kernel = value;
                divisor = 1;
                anchorX = SizeX / 2;
                anchorY = SizeY / 2;

                OnPropertyChanged(nameof(Kernel));
            }
        }

        public override string Name
        {
            get => "Generic (Netraul by default)";
            set
            {
                name = value;
            }
        }

    }
}




