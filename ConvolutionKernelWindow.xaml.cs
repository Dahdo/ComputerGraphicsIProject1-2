using System;
using System.Collections.Generic;
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

namespace ComputerGraphicsIProject
{
    public partial class ConvolutionKernelWindow : Window
    {
        private bool isInitializing = false; 
        MainWindow? mainWindow;
        public GenericFilter? genericConvolutioFilter;
        private int kernelCoeffTextBoxIndex = 0;

        // Controls
        ComboBox? predifinedFilterComboBox;
        ComboBox? nRowsComboBox;
        ComboBox? nColsComboBox;

        // Image bitmap
        Bitmap? initialImageSourceBitmap;


        public ConvolutionKernelWindow()
        {
            isInitializing = true;
            mainWindow = Application.Current.MainWindow as MainWindow;
            InitGenericConvolutionFilter();
            DataContext = genericConvolutioFilter;
            // Keep snapshot of the initial image bitmap
            initialImageSourceBitmap = mainWindow!.ImageSourceBitmap!.Clone() as Bitmap;
            InitializeComponent();
            InitControlHandles();
            isInitializing = false;
        }

        private void InitControlHandles()
        {
            predifinedFilterComboBox = FindName("PredifinedFilterComboBox") as ComboBox;
            nRowsComboBox = FindName("NRowsComboBox") as ComboBox;
            nColsComboBox = FindName("NColsComboBox") as ComboBox;

            int[] kernelDims = new[] { 1, 3, 5, 7, 9 };
            nRowsComboBox!.ItemsSource = kernelDims;
            nColsComboBox!.ItemsSource = kernelDims;
            
            nRowsComboBox.SelectedIndex = 1;
            nColsComboBox.SelectedIndex = 1;
        }

        private void InitGenericConvolutionFilter()
        {
            genericConvolutioFilter = new GenericFilter();
        }

        private void ApplyConvolutionFilter()
        {
            if (genericConvolutioFilter == null)
                return;

            // Call ApplyFilter
            ConvolutionFilters.ApplyFilter(mainWindow!.ImageSourceBitmap, genericConvolutioFilter);
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

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            RestoreBitmap();
        }

        private void OffsetTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Offset is already bound to the object. This method would only be useful during real-time modification
            // which will be added later on.
            if (isInitializing) // To prevent firing handling event during initialization
                return;
            //RestoreBitmap();
            //ApplyConvolutionFilter();
        }

        private void AnchorX_TextChanged(object sender, TextChangedEventArgs e)
        {
            // AnchorX is already bound to the object. This method would only be useful during real-time modification
            // which will be added later on.
            if (isInitializing) // To prevent firing handling event during initialization
                return;
            //ApplyConvolutionFilter();
        }

        private void AnchorY_TextChanged(object sender, TextChangedEventArgs e)
        {
            // AnchorY is already bound to the object. This method would only be useful during real-time modification
            // which will be added later on.
            if (isInitializing) // To prevent firing handling event during initialization
                return;
            //ApplyConvolutionFilter();
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
                Tuple<int, int> index2D = Util.ConvertTo2DIndex(index, genericConvolutioFilter!.SizeX);
                if (float.TryParse(textBox.Text, out float result))
                {
                    float[,]? tmpKernel = genericConvolutioFilter.Kernel.Clone() as float[,];
                    tmpKernel![index2D.Item1, index2D.Item2] = result;
                    genericConvolutioFilter.Kernel = tmpKernel;
                    kernelCoeffTextBoxIndex = 0; // Reset since the textboxes are reloaded
                }
            }
        }

        private void NRowsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isInitializing) // To prevent firing handling event during initialization
                return;
            if (nRowsComboBox!.SelectedItem == null) 
                return;
            kernelCoeffTextBoxIndex = 0; // Reset TextBoxes index
            int selectedRowSize = (int)nRowsComboBox.SelectedItem;
            genericConvolutioFilter!.SizeY = selectedRowSize;
        }

        private void NColsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isInitializing) // To prevent firing handling event during initialization
                return;
            if (nColsComboBox!.SelectedItem == null)
                return;
            kernelCoeffTextBoxIndex = 0; // Reset TextBoxes index
            int selectedColSize = (int)nColsComboBox.SelectedItem;
            genericConvolutioFilter!.SizeX = selectedColSize;
        }

        private void CalculateDivisorButton_Click(object sender, RoutedEventArgs e)
        {
            genericConvolutioFilter!.CalculateDivisor();
        }
    }
}
