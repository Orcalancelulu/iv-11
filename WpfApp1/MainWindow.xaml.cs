using System;
using System.Windows;
using System.Windows.Shapes;
using WpfApp1.Core;
using WpfApp1.MVVM.ViewModel;


namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>/// 
    public partial class MainWindow : Window
    {
        
        public MainWindow()
        {
            InitializeComponent();
            Button1.Visibility = Visibility.Collapsed;

            //connect to device
            bluetoothConnect();
        }

        private async void bluetoothConnect()
        {
            BtLE.connect("IV-11_DISPLAY");
            await BtLE.waitTillConnected();
            //connected with device
            Button1.Visibility = Visibility.Visible;
        }

        private async void sendData()
        {

        }
 
        private void loadHome(object sender, RoutedEventArgs e)
        {

            Button1.Visibility = Visibility.Collapsed;
            
            
           

        }
    }
}
