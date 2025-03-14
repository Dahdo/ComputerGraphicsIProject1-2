﻿using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace ComputerGraphicsIProject
{
    public class BitmapToBitmapImageConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is System.Drawing.Bitmap bitmap)
            {
                BitmapImage bitmapImage = new BitmapImage();

                // Convert System.Drawing.Bitmap to BitmapImage
                var memoryStream = new System.IO.MemoryStream();
                try
                {
                    bitmap.Save(memoryStream, bitmap.RawFormat);
                }
                catch
                {
                    bitmap.Save(memoryStream, ImageFormat.Bmp);
                }
                
                memoryStream.Position = 0;

                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memoryStream;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();

                return bitmapImage;
            }
            return null;


        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class Kernel2DArrayConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is float[,] twoDArray)
            {
                List<float[]> collection = new List<float[]>();

                for (int i = 0; i < twoDArray.GetLength(0); i++)
                {
                    float[] row = new float[twoDArray.GetLength(1)];

                    for (int j = 0; j < twoDArray.GetLength(1); j++)
                    {
                        row[j] = twoDArray[i, j];
                    }

                    collection.Add(row);
                }
                
                return collection;
            }

            return Binding.DoNothing;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is List<float[]> collection)
            {
                int rows = collection.Count;
                int cols = collection.Any() ? collection[0].Length : 0;

                float[,] twoDArray = new float[rows, cols];

                for (int i = 0; i < rows; i++)
                {
                    for (int j = 0; j < cols; j++)
                    {
                        twoDArray[i, j] = collection[i][j];
                    }
                }
                return twoDArray;
            }

            return Binding.DoNothing;
        }
    }

    public class ElementConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is float floatValue)
            {
                return floatValue.ToString();
            }

            return Binding.DoNothing;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string stringValue && float.TryParse(stringValue, out float result))
            {
                return result;
            }

            return Binding.DoNothing;
        }
    }

    public class ByteToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is byte)
            {
                return ((byte)value).ToString();
            }
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            byte result;
            if (byte.TryParse(value as string, out result))
            {
                return result;
            }
            return 0;
        }
    }

    public class IntToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int)
            {
                return ((int)value).ToString();
            }
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int result;
            if (int.TryParse(value as string, out result))
            {
                return result;
            }
            return 0;
    }
}

}