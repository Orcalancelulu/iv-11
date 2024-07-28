using System;
using System.Windows;
using System.Windows.Shapes;
using WpfApp1.Core;


namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();


            
        }



        private void loadHome(object sender, RoutedEventArgs e)
        {

            Button1.Visibility = Visibility.Collapsed;
            
            
           

        }
    }
}
