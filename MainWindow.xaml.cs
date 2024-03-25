using Microsoft.Win32;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using static ComputerGraphicsIProject.ErrorDiffusionDithering;


namespace ComputerGraphicsIProject
{

    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        // Constants
        const short BRIGHTNESS_ADJUSTMENT = -5;
        const float CONTRAST_ENHANCEMENT_PERCENTAGE = 10.0f;
        const float GAMMA_VALUE = 0.9f;

        private byte _numColorLevel = 2;
        public byte numColorLevel
        {
            get { return _numColorLevel; }
            set
            {
                if (_numColorLevel != value)
                {
                    _numColorLevel = value;
                    OnPropertyChanged(nameof(numColorLevel));
                }
            }
        }

        private int selectedChannel = -1;

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

        public void ReflectBitmapMemoryChanges()
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
                Filter = "All Image Files|*.png;*.jpg;*.bmp|PNG Image (*.png)|*.png|JPEG Image (*.jpg)|*.jpg|BMP Image (*.bmp)|*.bmp"
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
            FunctionalFilters.ApplyFilter(ImageSourceBitmap, FunctionalFilters.Inversion, selectedChannel);
            
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
            FunctionalFilters.ApplyFilter(ImageSourceBitmap, (byte channelValue) => FunctionalFilters.BrightnessCorrection(channelValue, BRIGHTNESS_ADJUSTMENT), selectedChannel);

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
            FunctionalFilters.ApplyFilter(ImageSourceBitmap, (byte channelValue) => FunctionalFilters.ConstrastEnhancement(channelValue, CONTRAST_ENHANCEMENT_PERCENTAGE), selectedChannel);

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
            FunctionalFilters.ApplyFilter(ImageSourceBitmap, (byte channelValue) => FunctionalFilters.GammaCorrection(channelValue, GAMMA_VALUE), selectedChannel);

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

            // Call ApplyFilter
            ConvolutionFilters.ApplyFilter(ImageSourceBitmap, blur);

            // To simulate bitmap changes notification
            ReflectBitmapMemoryChanges();
        }

        private void GaussianBlur_Click(object sender, RoutedEventArgs e)
        {
            if (ImageSourceBitmap == null)
            {
                Util.ShowMessageBoxError("Image needs to be loaded first!");
                return;
            }

            ConvolutionFilterBase gaussianBlur = new GaussianBlurFilter();

            gaussianBlur.Kernel = Util.NormalizeKernel(new float[,] 
                                                        {
                                                            {0, 1, 0},
                                                            {1, 4, 1},
                                                            {0, 1, 0}
                                                        });

            // Call ApplyFilter
            ConvolutionFilters.ApplyFilter(ImageSourceBitmap, gaussianBlur);

            // To simulate bitmap changes notification
            ReflectBitmapMemoryChanges();
        }

        private void Sharpen_Click(object sender, RoutedEventArgs e)
        {
            if (ImageSourceBitmap == null)
            {
                Util.ShowMessageBoxError("Image needs to be loaded first!");
                return;
            }

            ConvolutionFilterBase sharpen = new SharpenFilter();

            // Call ApplyFilter
            ConvolutionFilters.ApplyFilter(ImageSourceBitmap, sharpen);

            // To simulate bitmap changes notification
            ReflectBitmapMemoryChanges();
        }


        private void EdgeDetection_Click(object sender, RoutedEventArgs e)
        {
            if (ImageSourceBitmap == null)
            {
                Util.ShowMessageBoxError("Image needs to be loaded first!");
                return;
            }

            ConvolutionFilterBase edgeDetection = new EdgeDetectionFilter();

            // Call ApplyFilter
            ConvolutionFilters.ApplyFilter(ImageSourceBitmap, edgeDetection);

            // To simulate bitmap changes notification
            ReflectBitmapMemoryChanges();
        }

        private void Emboss_Click(object sender, RoutedEventArgs e)
        {
            if (ImageSourceBitmap == null)
            {
                Util.ShowMessageBoxError("Image needs to be loaded first!");
                return;
            }

            ConvolutionFilterBase emboss = new EmbossFilter();

            // Call ApplyFilter
            ConvolutionFilters.ApplyFilter(ImageSourceBitmap, emboss);

            // To simulate bitmap changes notification
            ReflectBitmapMemoryChanges();
        }
        #endregion

        private void ConvolutionKernel_Click(object sender, RoutedEventArgs e)
        {
            if (ImageSourceBitmap == null)
            {
                Util.ShowMessageBoxError("Image needs to be loaded first!");
                return;
            }

            ConvolutionKernelWindow convolutionKernelWindow = new ConvolutionKernelWindow();
            convolutionKernelWindow.Owner = this;
            convolutionKernelWindow.ShowDialog();
        }

        private void RgbToHsvbutton_Click(object sender, RoutedEventArgs e)
        {
            if (ImageSourceBitmap == null)
            {
                Util.ShowMessageBoxError("Image needs to be loaded first!");
                return;
            }
            FunctionalFilters.ApplyFilterToHSV(ImageSourceBitmap);
            // To simulate bitmap changes notification
            ReflectBitmapMemoryChanges();
        }

        private void HsvToRgbbutton_Click(object sender, RoutedEventArgs e)
        {
            if (ImageSourceBitmap == null)
            {
                Util.ShowMessageBoxError("Image needs to be loaded first!");
                return;
            }
            FunctionalFilters.ApplyFilterToRGB(ImageSourceBitmap);
            // To simulate bitmap changes notification
            ReflectBitmapMemoryChanges();
        }

        
        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            // Retrieve the selected radio button
            if (sender is RadioButton radioButton && radioButton.IsChecked == true)
            {
                string selectedOption = radioButton.Content.ToString();
                switch(selectedOption)
                {
                    case "red":
                        selectedChannel = 2;
                        break;
                    case "green":
                        selectedChannel = 1;
                        break;
                    case "blue":
                        selectedChannel = 0;
                        break;
                    default:
                        selectedChannel = 0;
                        break;

                }
            }
        }

        private void ChannelSelectionResetButton_Click(object sender, RoutedEventArgs e)
        {
            selectedChannel = -1;
            RadioButton? radio1 =  FindName("radioButton1") as RadioButton;
            RadioButton? radio2 = FindName("radioButton2") as RadioButton;
            RadioButton? radio3 = FindName("radioButton3") as RadioButton;

            radio1!.IsChecked = false;
            radio2!.IsChecked = false;
            radio3!.IsChecked = false;
        }

        private void RGBToGrayscaleButton_Click(object sender, RoutedEventArgs e)
        {
            if (ImageSourceBitmap == null)
            {
                Util.ShowMessageBoxError("Image needs to be loaded first!");
                return;
            }
            Util.RgbToGrayScale(ImageSourceBitmap);
            // To simulate bitmap changes notification
            ReflectBitmapMemoryChanges();
        }

        private void FloydAndSteinbergBtn_Click(object sender, RoutedEventArgs e)
        {
            if (ImageSourceBitmap == null)
            {
                Util.ShowMessageBoxError("Image needs to be loaded first!");
                return;
            }
            FloydAndSteinbergFilter floydAndSteinbergFilter = new FloydAndSteinbergFilter();
            ErrorDiffusionDithering.ApplyErrorDiffusion(ImageSourceBitmap, floydAndSteinbergFilter, numColorLevel);
            // To simulate bitmap changes notification
            ReflectBitmapMemoryChanges();
        }

        private void BurkesFilterBtn_Click(object sender, RoutedEventArgs e)
        {
            if (ImageSourceBitmap == null)
            {
                Util.ShowMessageBoxError("Image needs to be loaded first!");
                return;
            }
            BurkesFilter burkesFilter = new BurkesFilter();
            ErrorDiffusionDithering.ApplyErrorDiffusion(ImageSourceBitmap, burkesFilter, numColorLevel);
            // To simulate bitmap changes notification
            ReflectBitmapMemoryChanges();
        }

        private void StuckyFilterBtn_Click(object sender, RoutedEventArgs e)
        {
            if (ImageSourceBitmap == null)
            {
                Util.ShowMessageBoxError("Image needs to be loaded first!");
                return;
            }
            StuckyFilter stuckyFilter = new StuckyFilter();
            ErrorDiffusionDithering.ApplyErrorDiffusion(ImageSourceBitmap, stuckyFilter, numColorLevel);
            // To simulate bitmap changes notification
            ReflectBitmapMemoryChanges();
        }

        private void SierraFilterBtn_Click(object sender, RoutedEventArgs e)
        {
            if (ImageSourceBitmap == null)
            {
                Util.ShowMessageBoxError("Image needs to be loaded first!");
                return;
            }
            SierraFilter sierraFilter = new SierraFilter();
            ErrorDiffusionDithering.ApplyErrorDiffusion(ImageSourceBitmap, sierraFilter, numColorLevel);
            // To simulate bitmap changes notification
            ReflectBitmapMemoryChanges();
        }

        private void AtkinsonFilterBtn_Click(object sender, RoutedEventArgs e)
        {
            if (ImageSourceBitmap == null)
            {
                Util.ShowMessageBoxError("Image needs to be loaded first!");
                return;
            }
            AtkinsonFilter atkinsonFilter = new AtkinsonFilter();
            ErrorDiffusionDithering.ApplyErrorDiffusion(ImageSourceBitmap, atkinsonFilter, numColorLevel);
            // To simulate bitmap changes notification
            ReflectBitmapMemoryChanges();
        }

        private void ErrorDiffusion_Click(object sender, RoutedEventArgs e)
        {
            if (ImageSourceBitmap == null)
            {
                Util.ShowMessageBoxError("Image needs to be loaded first!");
                return;
            }

            DitheringWindow convolutionKernelWindow = new DitheringWindow();
            convolutionKernelWindow.Owner = this;
            convolutionKernelWindow.ShowDialog();
        }

        private void ColorQuantizationBtn_Click(object sender, RoutedEventArgs e)
        {
            if (ImageSourceBitmap == null)
            {
                Util.ShowMessageBoxError("Image needs to be loaded first!");
                return;
            }
            ColorQuantization.PopularityQuantization(ImageSourceBitmap, 25);
            // To simulate bitmap changes notification
            ReflectBitmapMemoryChanges();
        }
    }
}