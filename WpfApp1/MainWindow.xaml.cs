using System;
using System.Windows;
using System.Windows.Shapes;
using Windows.Devices.Bluetooth;
using WpfApp1.Core;
using WpfApp1.MVVM.ViewModel;


namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>/// 
    public partial class MainWindow : Window
    {
        private MainViewModel mainViewModel;


        public MainWindow()
        {
            InitializeComponent();
            mainViewModel = new MainViewModel();
            this.DataContext = mainViewModel;

            Button1.Visibility = Visibility.Collapsed;
            loadingText.Visibility = Visibility.Visible;
            loadingAnim.Visibility = Visibility.Visible;



            //connect to device
            bluetoothConnect();
        }

        private async void bluetoothConnect()
        {
            
            BtLE.connect("IV-11_DISPLAY");
            await BtLE.waitTillConnected();
            //connected with device

            Application.Current.Dispatcher.BeginInvoke(new Action(() => {
                //Button1.Visibility = Visibility.Visible;
                loadingAnim.Visibility = Visibility.Collapsed;
                //loadingText.Text = "Device Found";
                loadingText.Visibility = Visibility.Collapsed;
            }));

            mainViewModel.CurrentView = mainViewModel.HomeVM;
            BtLE.BluetoothDevice.ConnectionStatusChanged += OnBtDisconnect;
           
        }


        private void goBackToConnect()
        {
            mainViewModel.CurrentView = mainViewModel.ConnectVM;
            Application.Current.Dispatcher.BeginInvoke(new Action(() => {
                Button1.Visibility = Visibility.Collapsed;
                loadingText.Visibility = Visibility.Visible;
                loadingAnim.Visibility = Visibility.Visible;
                loadingText.Text = "Searching for Device...";

            }));
            bluetoothConnect();
        }

        private void OnBtDisconnect(BluetoothLEDevice bd, object e)
        {

            if (BtLE.BluetoothDevice.ConnectionStatus == BluetoothConnectionStatus.Disconnected)
            {
                BtLE.IsConnected = false;
                goBackToConnect();
            }
        }

        private void DragWindow(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void Minimize(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void CloseApp(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }


        private void loadHome(object sender, RoutedEventArgs e)
        {

            Button1.Visibility = Visibility.Collapsed;
            loadingText.Visibility = Visibility.Collapsed;



        }
    }
}
