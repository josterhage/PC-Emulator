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
            byte tempAL;
            byte tempAH;

            ushort tempA
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

            byte tempBL;
            byte tempBH;

            ushort tempB
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

            ushort tempC;
        }
    }
}
