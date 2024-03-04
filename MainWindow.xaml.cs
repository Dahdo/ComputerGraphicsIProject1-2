using Microsoft.Win32;
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

        // Constants
        const short BRIGHTNESS_ADJUSTMENT = -5;
        const float CONTRAST_ENHANCEMENT_PERCENTAGE = 10.0f;
        const float GAMMA_VALUE = 0.9f;

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

        private void ReflectBitmapMemoryChanges()
        {
            // Update the bitmap to trigger changes in the view
            Bitmap? tmpBitmap = ImageSourceBitmap;
            ImageSourceBitmap = null;
            ImageSourceBitmap = tmpBitmap;
        }
        #region NonFunction eventhandlers

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
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
        #endregion

        #region FunctionFilters eventhandlers
        private void Inversion_Click(object sender, RoutedEventArgs e)
        {
            if (ImageSourceBitmap == null)
            {
                Util.ShowMessageBoxError("Image needs to be loaded first!");
                return;
            }

            // Call inverstion filter with Inversion delegate
            FunctionalFilters.ApplyFilter(ImageSourceBitmap, FunctionalFilters.Inversion);
            
            // To simulate bitmap changes notification
            ReflectBitmapMemoryChanges();
        }

        private void BrightnessCorrection_Click(object sender, RoutedEventArgs e)
        {
            if (ImageSourceBitmap == null)
            {
                Util.ShowMessageBoxError("Image needs to be loaded first!");
                return;
            }

            // Call brightness correction filter with BrightnessCorrection delegate
            FunctionalFilters.ApplyFilter(ImageSourceBitmap, (byte channelValue) => FunctionalFilters.BrightnessCorrection(channelValue, BRIGHTNESS_ADJUSTMENT));

            // To simulate bitmap changes notification
            ReflectBitmapMemoryChanges();
        }

        private void ContrastEnhancement_Click(object sender, RoutedEventArgs e)
        {
            if (ImageSourceBitmap == null)
            {
                Util.ShowMessageBoxError("Image needs to be loaded first!");
                return;
            }

            // Call contrast enhancement filter with ConstrastEnhancement delegate
            FunctionalFilters.ApplyFilter(ImageSourceBitmap, (byte channelValue) => FunctionalFilters.ConstrastEnhancement(channelValue, CONTRAST_ENHANCEMENT_PERCENTAGE));

            // To simulate bitmap changes notification
            ReflectBitmapMemoryChanges();
        }

        private void GammaCorrection_Click(object sender, RoutedEventArgs e)
        {
            if (ImageSourceBitmap == null)
            {
                Util.ShowMessageBoxError("Image needs to be loaded first!");
                return;
            }

            // Call gamma correction filter with GammaCorrection delegate
            FunctionalFilters.ApplyFilter(ImageSourceBitmap, (byte channelValue) => FunctionalFilters.GammaCorrection(channelValue, GAMMA_VALUE));

            // To simulate bitmap changes notification
            ReflectBitmapMemoryChanges();
        }
        #endregion

        #region ConvolutionalFilters
        private void Blur_Click(object sender, RoutedEventArgs e)
        {
            if (ImageSourceBitmap == null)
            {
                Util.ShowMessageBoxError("Image needs to be loaded first!");
                return;
            }

            ConvolutionFilterBase blur = new BlurFilter();
            // Call inverstion filter with Inversion delegate
            ConvolutionFilters.ApplyFilter(ImageSourceBitmap, blur);

            // To simulate bitmap changes notification
            ReflectBitmapMemoryChanges();
        }
        #endregion

    }
}
