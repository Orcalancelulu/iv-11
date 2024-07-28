using LibreHardwareMonitor.Hardware;
using System.IO.Packaging;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Globalization;
using WpfApp1.Core;

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

        private Bulb[] bulbs;
        hardwareMonitor hw;

        public HomeView()
        {
            InitializeComponent();

            bulbs = new Bulb[4];
            bulbs[0] = new Bulb(1, ' ', this);
            bulbs[1] = new Bulb(2, ' ', this);
            bulbs[2] = new Bulb(3, ' ', this);
            bulbs[3] = new Bulb(4, ' ', this);

            //get data to display
            

            hw = new hardwareMonitor();
            System.Diagnostics.Debug.WriteLine("Sensor: {0}, value: {1}", "CPU Temperature", hw.Monitor(SensorType.Temperature, sensorName));

            //send data to raspberry





            //test
            displayTemperature();
            //end test


        }

        public void updateDisplay()
        {
            if (isClock)
            {
                /*DateTime localDate = DateTime.Now;
                String cultureName = "de-DE";

                
                var culture = new CultureInfo(cultureName);
                Console.WriteLine(localDate.ToString(culture));
                */
                System.Diagnostics.Debug.WriteLine(DateTime.Now.TimeOfDay);

                //DateTime.Now.TimeOfDay.ToString();

                bulbs[0].content = DateTime.Now.TimeOfDay.ToString()[0];

                bulbs[1].content = DateTime.Now.TimeOfDay.ToString()[1];
                bulbs[1].withPoint = true;

                bulbs[2].content = DateTime.Now.TimeOfDay.ToString()[3];

                bulbs[3].content = DateTime.Now.TimeOfDay.ToString()[4];

                foreach (Bulb bulb in bulbs)
                {
                    bulb.displayNumber();
                }

            } else
            {
                float number = hw.Monitor(sensorType, sensorName);

                string value = ((int)Math.Round(number, 0)).ToString();
                if (value.Length <= 1 || (value.Length <= 2 && dataDisplayLength == 3)) value = " " + value;

                System.Diagnostics.Debug.WriteLine(value);

                bulbs[0].content = (char)value[0];
                if (sensorType == SensorType.Clock) bulbs[0].withPoint = true;
                bulbs[1].content = (char)value[1];
                bulbs[2].content = (dataDisplayLength == 2) ? units[0] : (char)value[2];
                bulbs[3].content = units[units.Length - 1];

                foreach (Bulb bulb in bulbs)
                {
                    bulb.displayNumber();
                }
            }
        }


        public void displayTemperature() //2 digit data, 2 digit units
        {
            System.Diagnostics.Debug.WriteLine("temp button");

            sensorName = "CPU Package";
            sensorType = SensorType.Temperature;

            units = ['°', 'C'];
            dataDisplayLength = 2;

            isClock = false;

            updateDisplay();

        }
        public void displayPower() //3 digits data, 1 digit units
        {
            System.Diagnostics.Debug.WriteLine("power button");

            sensorName = "CPU Package";
            sensorType = SensorType.Power;

            units = ['W'];
            dataDisplayLength = 3;

            isClock = false;


            updateDisplay();
        }
        public void displayClock() //2 digit data, 2 digit units
        {
            System.Diagnostics.Debug.WriteLine("clock button");

            sensorName = "CPU Core #1";
            sensorType = SensorType.Clock;

            units = ['G', 'H'];
            dataDisplayLength = 2;

            isClock = false;


            updateDisplay();
        }

        public void displayLoad() //2 digit data, 2 digit units
        {
            System.Diagnostics.Debug.WriteLine("load button");


            sensorName = "CPU Total";
            sensorType = SensorType.Load;

            units = ['P', 'r'];
            dataDisplayLength = 2;

            isClock = false;


            updateDisplay();
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
            System.Diagnostics.Debug.WriteLine("wanting time");
           isClock = true;
            updateDisplay(); 
        }




    }
}
