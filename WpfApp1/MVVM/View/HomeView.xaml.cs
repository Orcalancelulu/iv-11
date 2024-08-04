using LibreHardwareMonitor.Hardware;
using System.Windows.Controls;
using WpfApp1.Core;
using System.Timers;
using System.Threading;
using System.Windows.Media;
using System.Windows;
using System.Reflection.PortableExecutable;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Storage.Streams;
using HidSharp.Experimental;
using HidSharp.Utility;
using System.Xml.Linq;


namespace WpfApp1.MVVM.View
{
    /// <summary>
    /// Interaktionslogik für HomeView.xaml
    /// </summary>
    public partial class HomeView : UserControl
    {

        private string sensorName;
        private SensorType sensorType;

        private char[] units = new char[2];
        private int dataDisplayLength;
        private bool isClock = false;

        private System.Timers.Timer timer;

        private bool isSecondPointActive = false;

        private Bulb[] bulbs;
        hardwareMonitor hw;
        private int displayIndex = 0;

        private bool isHWRunning = false;

        private string[] displayInfoString = new string[5] { "CPU Temperature: {0}°C", "CPU Power: {0}W", "CPU Clock Speed: {0}GHz", "CPU Load: {0}%", "{0}"};

        private const int numberOfRadioButtons = 5;
        private int activeRadioButton = 1;

        public HomeView()
        {
            InitializeComponent();

            bulbs = new Bulb[4];
            for (int i = 0; i < bulbs.Length; i++)
            {
                bulbs[i] = new Bulb(i + 1, ' ', this);
            }
            hw = new hardwareMonitor();

            restartTimer(5000);
            displayTemperature(); //sets standard display to cpu temp

            System.Diagnostics.Debug.WriteLine(BtLE.IsConnected.ToString());
            setUpNotification();

        }

        private async void setUpNotification()
        {
            GattCommunicationStatus status = await BtLE.SettingsCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.Notify);
            BtLE.SettingsCharacteristic.ValueChanged += displayInfoChange;
        }

        private void displayInfoChange(GattCharacteristic sender, GattValueChangedEventArgs args)
        {
            //value of the characteristics is not important, just the noitify event
            activeRadioButton += 1;
            if (activeRadioButton > numberOfRadioButtons) { activeRadioButton = 1; }

            System.Diagnostics.Debug.WriteLine("active button = " + activeRadioButton);
            Application.Current.Dispatcher.BeginInvoke(new Action(() => ((RadioButton)this.FindName("rb" + activeRadioButton)).IsChecked = true));

            if (activeRadioButton == 1) displayTemperature();
            else if (activeRadioButton == 2) displayPower();
            else if (activeRadioButton == 3) displayClock();
            else if (activeRadioButton == 4) displayLoad();
            else if (activeRadioButton == 5) displayTime();

        }

        private void restartTimer(int pause)
        {
            if (timer != null) timer.Dispose();
            timer = new System.Timers.Timer(pause);
            timer.Elapsed += new ElapsedEventHandler(timerCall);
            timer.Enabled = true;
        }


        private void timerCall(object source, ElapsedEventArgs e)
        {
            //System.Diagnostics.Debug.WriteLine("running timer " + e.SignalTime);
            updateDisplayCheck();
        }

        private async void updateDisplayCheck() //only one hw.Monitor is allowed to run at the same time or else it crashes
        {
            if (isClock)
            {
                updateDisplay();
                return;
            } else
            {
                if (!isHWRunning)
                {
                    //hw is method is not running, can call updateDisplay
                    isHWRunning = true;
                    await Task.Run(updateDisplay);
                    isHWRunning = false;
                    //System.Diagnostics.Debug.WriteLine("Running display");

                }
            }
        }


        public void updateDisplay()
        {
            if (isClock)
            {
                string currentTime = DateTime.Now.TimeOfDay.ToString(); //returns: xx:xx
                bulbs[0].Content = currentTime[0];
                if (isSecondPointActive)
                {
                    isSecondPointActive = false;
                    bulbs[1].withPoint = false;
                }
                else
                {
                    isSecondPointActive = true;
                    bulbs[1].withPoint = true;
                }
                bulbs[1].Content = currentTime[1];
                bulbs[2].Content = currentTime[3];
                bulbs[3].Content = currentTime[4];


                Application.Current.Dispatcher.BeginInvoke(new Action(() => displayInfo.Text = String.Format(displayInfoString[displayIndex], DateTime.Now)));


            }
            else
            {
                SensorType sensorTypeBeforeHW = sensorType;
                float number = hw.Monitor(sensorType, sensorName);
                if (sensorTypeBeforeHW != sensorType) //hw.monitor takes a long time, especially on slow computers, therefore, if the user changes the sensor during the loading of another, the data wont be displayed
                {
                    updateDisplay();
                    return;
                }
                string value = ((int)Math.Round(number, 0)).ToString();

                string valueToShowAsText = value;
                if (displayIndex == 2 && value.Length > 2)
                {
                    System.Diagnostics.Debug.WriteLine(value);
                    valueToShowAsText = value[0] + "." + value[1] + value[2];
                }

                Application.Current.Dispatcher.BeginInvoke(new Action(() => displayInfo.Text = String.Format(displayInfoString[displayIndex], valueToShowAsText)));

                if (value.Length <= 1 || (value.Length <= 2 && dataDisplayLength == 3)) value = " " + value;
                if (value.Length <= 2 && dataDisplayLength == 3) value = " " + value;
                //System.Diagnostics.Debug.WriteLine(value);

                if (sensorType == SensorType.Clock) bulbs[0].withPoint = true;
                bulbs[0].Content = (char)value[0];
                bulbs[1].Content = (char)value[1];
                bulbs[2].Content = (dataDisplayLength == 2) ? units[0] : (char)value[2];
                bulbs[3].Content = units[units.Length - 1];

                
            }
            sendDataToIV11();
        }

        private async void sendDataToIV11()
        {
            

            charTo7Seg table = new charTo7Seg();
            List<int[]> byteInts = new List<int[]>();

            foreach (Bulb bulb in bulbs)
            {
                byteInts.Add(table.getTable(bulb.Content, bulb.withPoint));   
            }


            List<int> Pow2 = new List<int> { 128, 64, 32, 16, 8, 4, 2, 1 };
            byte[] byteArray = new byte[4];

            for (int b = 0; b < 4; b++)
            {
                int byteResult = 0;
                int[] byteInt = byteInts[b];

                for (int i = 0; i < byteInt.Length; i++)
                {
                    byteResult += (byteInt[i] == 0) ? 0 : Pow2[i];
                }

                byteArray[b] = Convert.ToByte(byteResult);
            }

            var writer = new DataWriter();

            writer.WriteBytes(byteArray);

            GattCommunicationStatus resultWrite = await BtLE.DataCharacteristic.WriteValueAsync(writer.DetachBuffer());
            if (resultWrite == GattCommunicationStatus.Success)
            {
                // Successfully wrote to device
                Console.WriteLine("wrote to device");

            }
        }


        public void displayTemperature() //2 digit data, 2 digit units
        {
            activeRadioButton = 1;

            //reset display
            foreach (Bulb bulb in bulbs)
            {
                bulb.withPoint = false;
                bulb.Content = '-';
            }
            sendDataToIV11();

            displayIndex = 0;
            Application.Current.Dispatcher.BeginInvoke(new Action(() => displayInfo.Text = String.Format(displayInfoString[displayIndex], "--")));


            //change filters so new sensor data is read
            System.Diagnostics.Debug.WriteLine("temp button");


            sensorName = "CPU Package";
            sensorType = SensorType.Temperature;

            //set display options
            units = ['°', 'C'];
            dataDisplayLength = 2;
            isClock = false;

            //update display now
            updateDisplayCheck();
            timer.Interval = 5000;

            //restartTimer(5000);
        }
        public void displayPower() //3 digits data, 1 digit units
        {
            activeRadioButton = 2;

            foreach (Bulb bulb in bulbs)
            {
                bulb.withPoint = false;
                bulb.Content = '-';
            }
            sendDataToIV11();

            System.Diagnostics.Debug.WriteLine("power button");

            displayIndex = 1;
            Application.Current.Dispatcher.BeginInvoke(new Action(() => displayInfo.Text = String.Format(displayInfoString[displayIndex], "--")));


            sensorName = "CPU Package";
            sensorType = SensorType.Power;

            units = ['W'];
            dataDisplayLength = 3;

            isClock = false;

            // if (hw == null) hw = new hardwareMonitor();

            updateDisplayCheck();
            timer.Interval = 5000;

            //restartTimer(5000);
        }
        public void displayClock() //2 digit data, 2 digit units
        {
            activeRadioButton = 3;

            foreach (Bulb bulb in bulbs)
            {
                bulb.withPoint = false;
                bulb.Content = '-';
            }
            sendDataToIV11();

            System.Diagnostics.Debug.WriteLine("clock button");

            displayIndex = 2;
            Application.Current.Dispatcher.BeginInvoke(new Action(() => displayInfo.Text = String.Format(displayInfoString[displayIndex], "--")));

            sensorName = "CPU Core #1";
            sensorType = SensorType.Clock;

            units = ['G', 'H'];
            dataDisplayLength = 2;

            isClock = false;

            // if (hw == null) hw = new hardwareMonitor();

            updateDisplayCheck();
            timer.Interval = 5000;

            //restartTimer(5000);
        }

        public void displayLoad() //2 digit data, 2 digit units
        {
            activeRadioButton = 4;

            foreach (Bulb bulb in bulbs)
            {
                bulb.withPoint = false;
                bulb.Content = '-';
            }
            sendDataToIV11();

            System.Diagnostics.Debug.WriteLine("load button");

            displayIndex = 3;
            Application.Current.Dispatcher.BeginInvoke(new Action(() => displayInfo.Text = String.Format(displayInfoString[displayIndex], "--")));

            sensorName = "CPU Total";
            sensorType = SensorType.Load;

            units = ['P', 'r'];
            dataDisplayLength = 2;

            isClock = false;

            // if (hw == null) hw = new hardwareMonitor();

            updateDisplayCheck();
            timer.Interval = 5000;

            //restartTimer(5000);
        }

        public void displayTime()
        {
            activeRadioButton = 5;

            foreach (Bulb bulb in bulbs)
            {
                bulb.withPoint = false;
                bulb.Content = '-';
            }
            sendDataToIV11();

            displayIndex = 4;

            System.Diagnostics.Debug.WriteLine("wanting time");
            isClock = true;
            updateDisplayCheck();
            timer.Interval = 1000;
        }


        private void Button_Click_1(object sender, EventArgs e)
        {
            
            displayTemperature();
        }

        private void Button_Click_2(object sender, EventArgs e)
        {
            displayPower();
        }

        private void Button_Click_3(object sender, EventArgs e)
        {
            displayClock();
        }

        private void Button_Click_4(object sender, EventArgs e)
        {
            displayLoad();
        }

        private void Button_Click_5(object sender, EventArgs e)
        {
            displayTime();
        }




    }
}
