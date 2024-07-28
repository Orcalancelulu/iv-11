using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1.Core
{
    class charTo7Seg
    {
        public Dictionary<char, int[]> cToInt { get; set; }
        public charTo7Seg()
        {
            cToInt = new Dictionary<char, int[]>();


            cToInt.Add(' ', [0, 0, 0, 0, 0, 0, 0, 0]);
            cToInt.Add('.', [0, 0, 0, 0, 0, 0, 0, 1]);


            cToInt.Add('0', [1, 1, 1, 1, 1, 1, 0, 0]);
            cToInt.Add('1', [0, 1, 1, 0, 0, 0, 0, 0]);
            cToInt.Add('2', [1, 1, 0, 1, 1, 0, 1, 0]);
            cToInt.Add('3', [1, 1, 1, 1, 0, 0, 1, 0]);
            cToInt.Add('4', [0, 1, 1, 0, 0, 1, 1, 0]);
            cToInt.Add('5', [1, 0, 1, 1, 0, 1, 1, 0]);
            cToInt.Add('6', [1, 0, 1, 1, 1, 1, 1, 0]);
            cToInt.Add('7', [1, 1, 1, 0, 0, 0, 0, 0]);
            cToInt.Add('8', [1, 1, 1, 1, 1, 1, 1, 0]);
            cToInt.Add('9', [1, 1, 1, 0, 0, 1, 1, 0]);

            cToInt.Add('°', [1, 1, 0, 0, 0, 1, 1, 0]);
            cToInt.Add('C', [1, 0, 0, 1, 1, 1, 0, 0]);

            cToInt.Add('W', [1, 0, 1, 1, 1, 0, 0, 0]);

            cToInt.Add('P', [1, 1, 0, 0, 1, 1, 1, 0]);
            cToInt.Add('r', [0, 0, 0, 0, 1, 0, 1, 0]);

            cToInt.Add('G', [1, 0, 1, 1, 1, 1, 0, 0]);
            cToInt.Add('H', [0, 1, 1, 0, 1, 1, 1, 0]);


        }

        public int[] getTable(char c, bool withPoint)
        {
            int[] data = cToInt[c];
            if (data == null) return [1, 0, 0, 1, 0, 0, 0, 0];


            if (withPoint) data[7] = 1;
            return data;


        }
    }
}
