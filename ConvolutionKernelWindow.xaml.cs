using System;
using System.Collections.Generic;
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
        MainWindow? mainWindow;
        public GenericFilter? GenericConvolutioFilter;

        public ConvolutionKernelWindow()
        {
            mainWindow = Application.Current.MainWindow as MainWindow;
            InitGenericConvolutionFilter();
            DataContext = GenericConvolutioFilter;
            InitializeComponent();
        }

        private void InitGenericConvolutionFilter()
        {
            GenericConvolutioFilter = new GenericFilter();
        }

        private void ApplyConvolutionFilter()
        {
            if (GenericConvolutioFilter == null)
                return;

            // Call ApplyFilter
            ConvolutionFilters.ApplyFilter(mainWindow!.ImageSourceBitmap, GenericConvolutioFilter);
            // To simulate bitmap changes notification
            mainWindow!.ReflectBitmapMemoryChanges();
        }

        private void RestoreBitmap()
        {
            // To implement soon
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
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
    }
}
