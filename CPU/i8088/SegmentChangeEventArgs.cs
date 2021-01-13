using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SystemBoard.i8088
{
    public class SegmentChangeEventArgs : EventArgs
    {
        public Segment Segment;
        public ushort Value;
        public byte[] SegmentMap;

        public SegmentChangeEventArgs(Segment s, ushort value, byte[] segmentMap)
        {
            Segment = s;
            Value = value;
            SegmentMap = segmentMap;
        }
    }
}
