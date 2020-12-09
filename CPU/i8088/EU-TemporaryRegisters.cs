using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPU.i8088
{
    public partial class Processor
    {
        private partial class ExecutionUnit
        {

            private byte tempAL =0;
            private byte tempAH =0;

            /// <summary>
            /// tempA is for data going *to* the bus
            /// </summary>
            private ushort TempA
            {
                get {
                    return (ushort)((tempAH << 8) | tempAL);
                }
                set
                {
                    tempAL = (byte)(value & 0x00FF);
                    tempAH = (byte)((value & 0xFF00) >> 8);
                }
            }

            private byte tempBL=0;
            private byte tempBH;

            /// <summary>
            /// tempB is for data coming *from* the bus
            /// tempBL is for segment override (extracted from the instruction code)
            /// </summary>
            private ushort TempB
            {
                get
                {
                    return (ushort)((tempBH << 8) | tempBL);
                }
                set
                {
                    tempBL = (byte)(value & 0x00FF);
                    tempBH = (byte)((value & 0xFF00) >> 8);
                }
            }

            private byte tempCL;
            private byte tempCH;

            /// <summary>
            /// tempC is for pointers
            /// </summary>
            private ushort TempC
            {
                get
                {
                    return (ushort)((tempCH << 8) | tempCL);
                }
                set
                {
                    tempCL = (byte)(value & 0x00FF);
                    tempCH = (byte)((value & 0xFF00) >> 8);
                }
            }
        }
    }
}
