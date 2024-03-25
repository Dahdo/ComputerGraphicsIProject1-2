using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static ComputerGraphicsIProject.ErrorDiffusionDithering;

namespace ComputerGraphicsIProject
{
    public partial class DitheringWindow : Window, INotifyPropertyChanged
    {
        private bool isInitializing = false;
        MainWindow? mainWindow;
        public ErrorDistributionMatrixBase? currentErrorDiffusionFilter;
        private int kernelCoeffTextBoxIndex = 0;
        private List<ErrorDistributionMatrixBase>? errorDiffusionFilterList;

        // Controls
        ComboBox? predifinedFilterComboBox;

        // Image bitmap
        Bitmap? initialImageSourceBitmap;

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
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        public DitheringWindow()
        {
            isInitializing = true;
            mainWindow = Application.Current.MainWindow as MainWindow;
            InitConvolutionFilters();
            DataContext = currentErrorDiffusionFilter;
            // Keep snapshot of the initial image bitmap
            initialImageSourceBitmap = mainWindow!.ImageSourceBitmap!.Clone() as Bitmap;
            InitializeComponent();
            InitControlHandles();
            isInitializing = false;
        }

        private void InitControlHandles()
        {
            predifinedFilterComboBox = FindName("PredifinedFilterComboBox") as ComboBox;
            predifinedFilterComboBox!.SelectedIndex = 0;

            foreach (var filter in errorDiffusionFilterList!)
            {
                predifinedFilterComboBox!.Items.Add(filter.Name);
            }
        }


        private void InitConvolutionFilters()
        {
            FloydAndSteinbergFilter floydAndSteinbergFilter = new FloydAndSteinbergFilter();

            errorDiffusionFilterList = new List<ErrorDistributionMatrixBase>
            {
                floydAndSteinbergFilter,
                new BurkesFilter(),
                new StuckyFilter(),
                new SierraFilter(),
                new AtkinsonFilter()
            };
            currentErrorDiffusionFilter = errorDiffusionFilterList[0];
        }

        private void ApplyConvolutionFilter()
        {
            if (currentErrorDiffusionFilter == null)
                return;

            // Call ApplyFilter
            ErrorDiffusionDithering.ApplyErrorDiffusion(mainWindow!.ImageSourceBitmap, currentErrorDiffusionFilter, numColorLevel);
            // To simulate bitmap changes notification
            mainWindow!.ReflectBitmapMemoryChanges();
        }

        private void RestoreBitmap()
        {
            // Restore the image bitmap to its initial state
            mainWindow!.ImageSourceBitmap = initialImageSourceBitmap!.Clone() as Bitmap;
        }

        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            ApplyConvolutionFilter(); // To be modified to just close the window since changes will real time
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            RestoreBitmap();
            this.Close();
        }


        private void DivisorTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Divisor is already bound to the object. This method would only be useful during real-time modification
            // which will be added later on.
            if (isInitializing) // To prevent firing handling event during initialization
                return;
            //ApplyConvolutionFilter();
        }

        private void KernelCoefficientTextBox_Loaded(object sender, RoutedEventArgs e)
        {
            TextBox? textBox = sender as TextBox;
            textBox!.Tag = kernelCoeffTextBoxIndex++;
        }
        private void KernelCoefficientTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (isInitializing) // To prevent firing handling event during initialization
                return;

            TextBox? textBox = (TextBox)sender;
            if (textBox != null && textBox.Tag != null)
            {
                int index = (int)textBox.Tag;
                Tuple<int, int> index2D = Util.ConvertTo2DIndex(index, currentErrorDiffusionFilter!.SizeX);
                if (float.TryParse(textBox.Text, out float result))
                {
                    float[,]? tmpKernel = currentErrorDiffusionFilter.Kernel.Clone() as float[,];
                    tmpKernel![index2D.Item1, index2D.Item2] = result;
                    currentErrorDiffusionFilter.Kernel = tmpKernel;
                    kernelCoeffTextBoxIndex = 0; // Reset since the textboxes are reloaded
                }
            }
        }

        private void PredifinedFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;
            if (comboBox.SelectedIndex != -1)
            {
                int selectedIndex = comboBox.SelectedIndex;

                currentErrorDiffusionFilter = errorDiffusionFilterList!.ElementAt(selectedIndex);
                this.DataContext = currentErrorDiffusionFilter;
            }
        }

        private void RefreshFilterList(ErrorDistributionMatrixBase newFilter)
        {
            InitConvolutionFilters();
            errorDiffusionFilterList!.Add(newFilter);
            predifinedFilterComboBox!.Items.Clear();
            foreach (var filter in errorDiffusionFilterList!)
            {
                predifinedFilterComboBox!.Items.Add(filter.Name);
            }
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            RestoreBitmap();
        }

        private void SaveFilterButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}

