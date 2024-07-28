using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfApp1.MVVM.View;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows.Controls;

namespace WpfApp1.Core
{

    class Bulb
    {
        public int bulbNumber { get; set; }
        public char content {
            get;
            set; 
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

            //missing: content to segmentArray
            charTo7Seg table = new charTo7Seg();
            int[] segmentArray = table.getTable(content, withPoint);


            int i = 0;
            foreach (string segment in segments)
            {
                var myRect = fwElement.FindName(segment) as Rectangle;
                if (myRect != null)
                {
                    if (segmentArray[i] == 0)
                    {
                        myRect.Fill = Brushes.DarkGray;
                    } else
                    {
                        myRect.Fill = Brushes.LawnGreen;
                    }
                    i++;
                }
            }


        }
    }
}
