using JTServer.GW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace _808GW.Models
{
    public class PagingCJView
    {
        public int GetLastIndex()
        {
            var I = Start - Data.Count;
            if (I < 0)
            {
                I = 0;
            }
            return I;
        }
        public int GetNextIndex()
        {
            return Start + Data.Count;
        }

        public int Sum { get; set; }
        public int Start { get; set; }

        public List<JTCheji> Data { get; set; }
    }
}
