using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SystemBoard.i8088
{
    public class BIUChangeEventArgs : EventArgs
    {
        public SegmentRegisters Segments;
        public ushort IP;

        public BIUChangeEventArgs(SegmentRegisters s, ushort ip)
        {
            Segments = s;
            IP = ip;
        }
    }
}
