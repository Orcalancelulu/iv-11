using System;
using System.Windows;
using WpfApp1.MVVM.View;
using System.Windows.Shapes;
using System.Windows.Media;

using System.Windows.Threading;

namespace WpfApp1.Core
{

    class Bulb
    {
        public int bulbNumber { get; set; }

        private char content;

        public char Content {
            get => content;
            set
            {
                content = value;
                displayNumber();
            } 
        }

        public bool withPoint { get; set; }

        public HomeView fwElement { get; set; }
        public string[] segments
        {
            get 
            {
                string[] segmentNames = new string[8];
                for (int i = 0; i<8 ; i++)
                {
                    segmentNames[i] = "t" + bulbNumber + "" + (i+1);
                }
                return segmentNames;
                    
            }
        }

        public Bulb(int _bulbNumber, char _content, HomeView _fwElement) 
        {
            bulbNumber = _bulbNumber;
            content = _content;
            fwElement = _fwElement as HomeView;
        }


        public void displayNumber()
        {
            //displays number

            charTo7Seg table = new charTo7Seg();
            int[] segmentArray = table.getTable(content, withPoint);


            int i = 0;
            foreach (string segment in segments)
            {

                if (segmentArray[i] == 0)
                {
                        
                    Application.Current.Dispatcher.BeginInvoke(new Action(() => ((Rectangle)fwElement.FindName(segment)).Fill = Brushes.DarkGray));
                } else
                {
                        
                    Application.Current.Dispatcher.BeginInvoke(new Action(() => ((Rectangle)fwElement.FindName(segment)).Fill = Brushes.LawnGreen));
                }
                i++;           
            }
        }
    }
}
