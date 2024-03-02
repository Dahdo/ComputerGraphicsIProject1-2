﻿using Microsoft.Win32;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Imaging;


namespace ComputerGraphicsIProject
{

    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private Bitmap? _imageSourceBitmap;

        public event PropertyChangedEventHandler? PropertyChanged;

        public Bitmap? ImageSourceBitmap
        {
            get { return _imageSourceBitmap; }
            set
            {
                if (_imageSourceBitmap != value)
                {
                    _imageSourceBitmap = value;
                    OnPropertyChanged(nameof(ImageSourceBitmap));
                }
            }
        }
        private Bitmap? originalImageSourceBitmap; // Original state of ImageSourceBitmap
        private System.Windows.Controls.Image? refImage; // Reference to reference image control

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        
        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            refImage = FindName("RefImage") as System.Windows.Controls.Image;
        }


        private void Open_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = "Open Image File",
                Filter = "PNG Image (*.png)|*.png|JPEG Image (*.jpg)|*.jpg|BMP Image (*.bmp)|*.bmp"
        };

            if (openFileDialog.ShowDialog() == true)
            {
                string selectedFilePath = openFileDialog.FileName;

                try
                {
                    originalImageSourceBitmap = new Bitmap(selectedFilePath); // The initial state of the bitmap
                    ImageSourceBitmap = originalImageSourceBitmap.Clone() as Bitmap; // The bitmap to apply filters on

                    if (refImage != null)
                    {
                        BitmapImage refBitmapImage = new BitmapImage(new Uri(selectedFilePath));
                        refImage.Source = refBitmapImage;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading image: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void RevertAll_Click(object sender, RoutedEventArgs e)
        {
            ImageSourceBitmap = originalImageSourceBitmap!.Clone() as Bitmap;
                
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if(ImageSourceBitmap == null)
            {
                Util.ShowMessageBoxError("No image loaded!");
                return;
            }
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "PNG Image (*.png)|*.png|JPEG Image (*.jpg)|*.jpg|BMP Image (*.bmp)|*.bmp|All files (*.*)|*.*";
            saveFileDialog.Title = "Save Image";

            if (saveFileDialog.ShowDialog() == true)
            {
                string fileName = saveFileDialog.FileName;
                // Call the function defined in Util class
                Util.SaveBitmapToFile(ImageSourceBitmap, fileName);
            }
        }


        private void Inversion_Click(object sender, RoutedEventArgs e)
        {
            if (ImageSourceBitmap == null)
            {
                Util.ShowMessageBoxError("Image needs to be loaded first!");
                return;
            }

            FunctionalFilters.ApplyInversionFilter(ImageSourceBitmap);

            // Update the bitmap to trigger changes in the view
            Bitmap? tmpBitmap = ImageSourceBitmap;
            ImageSourceBitmap = null;
            ImageSourceBitmap = tmpBitmap;
        }
    }
}
