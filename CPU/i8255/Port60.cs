using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SystemBoard.i8255
{
    public class Port60 : IPeripheral
    {
        private byte nextScanCode;
        private byte switch1Settings;

        public Port60(byte switch1Settings)
        {
            this.switch1Settings = switch1Settings;
        }

        public void SetSwitch1(byte switch1Settings)
        {
            this.switch1Settings = switch1Settings;
        }

        public byte Get()
        {
            byte result;
            if (nextScanCode > 0)
            {
                result = nextScanCode;
                nextScanCode = 0;
            }
            else
            {
                result = switch1Settings;
            }

            return result;
        }

        //port 60h isn't settable
        public void Set(byte data)
        {
            return;
        }
    }
}
