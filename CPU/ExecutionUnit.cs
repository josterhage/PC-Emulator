using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SystemClock;

namespace CPU
{
    public partial class ExecutionUnit
    {
        private readonly MainTimer mainTimer = MainTimer.GetInstance();
        private event EventHandler tick;

        private byte opcode = 0;

        // true = to xxx register
        // false = from xxx register
        private bool direction = false;
        private bool width = false;

        public byte Opcode
        {
            get
            {
                byte result = 0;
                result |= (byte)(opcode << 2);
                result |= direction ? 2 : 0;
                result |= width ? 1 : 0;
                return result;
            }
            set
            {
                width = (value & 1) != 0;
                direction = (value & 2) != 0;
                opcode = (byte)((value & 0b11111100) >> 2);
            }
        }

        private byte mod = 0;
        private byte reg = 0;
        private byte rm = 0;

        public byte ModRM
        {
            get
            {
                byte result = 0;
                result |= (byte)(mod << 6);
                result |= (byte)(reg << 3);
                result |= rm;
                return result;
            }
            set
            {
                mod = (byte)((value & 0b11000000) >> 6);
                reg = (byte)((value & 0b00111000) >> 3);
                rm =  (byte) (value & 0b00000111);
            }
        }

        private byte dataByteLow = 0;
        private byte dataByteHigh = 0;
        private byte addressLow = 0;
        private byte addressHigh = 0;

        private byte tempByte = 0;
        private ushort tempWord = 0;

        private readonly RegisterSet registers;
        private readonly BusInterfaceUnit busInterfaceUnit;

        public ExecutionUnit(RegisterSet registers, BusInterfaceUnit busInterfaceUnit)
        {
            this.registers = registers;
            this.busInterfaceUnit = busInterfaceUnit;
            initialize_instruction_set();
            mainTimer.TockEvent += on_tick;
            tick += fetch_opcode;
        }

        private void on_tick(object sender, EventArgs e)
        {
            tick?.Invoke(sender,e);
        }
    }
}
