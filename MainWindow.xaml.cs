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
        private Bitmap? _imageSource;
        Bitmap tmpBitmap;

        public event PropertyChangedEventHandler? PropertyChanged;

        public Bitmap? ImageSource
        {
            get { return _imageSource; }
            set
            {
                if (_imageSource != value)
                {
                    _imageSource = value;
                    OnPropertyChanged(nameof(ImageSource));
                }
            }
        }

        private System.Windows.Controls.Image? refImage;

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
                Filter = "Image Files|*.jpg;*.jpeg;*.png;*.gif;*.bmp|All Files|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string selectedFilePath = openFileDialog.FileName;

                try
                {
                    Bitmap? bitmap = new Bitmap(selectedFilePath);
                    ImageSource = bitmap;

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

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            FunctionalFilters.ApplyInversionFilter(ImageSource);

            // Update the bitmap to trigger changes in the view
            Bitmap? tmpBitmap = ImageSource;
            ImageSource = null;
            ImageSource = tmpBitmap;

        }

        private void RevertAll_Click(object sender, RoutedEventArgs e)
        {
        }
    }
}
