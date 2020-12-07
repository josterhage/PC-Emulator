using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SystemClock;

namespace CPU.i8088.ExecutionUnit
{
    public partial class ExecutionUnit
    {
        private readonly MainTimer mainTimer = MainTimer.GetInstance();
        private readonly GeneralRegisters registers = new GeneralRegisters();

        
        #region opcode_byte
        //refactor changes:
        // 'opcode' is now synonymous with 'Opcode', it is no longer the upper 6 bits
        // get no longer constructs the byte by OR'ing multiple values
        // set still alters 'direction' and 'width' but directly assigns 'opcode'
        private byte opcode = 0;

        // true = to xxx register
        // false = from xxx register
        private bool direction = false;
        private bool width = false;
        
        //TODO: should Opcode be accessible publicly?
        /// <summary>
        /// Opcode is one byte representing an 8086 instruction opcode or prefix
        /// </summary>
        public byte Opcode
        {
            get
            {
                return opcode;
            }
            private set
            {
                width = (value & 1) != 0;
                direction = (value & 2) != 0;
                opcode = value;
            }
        }
        #endregion

        #region modrm_byte
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
        #endregion

        private byte dataByteLow = 0; 
        private byte dataByteHigh = 0;
        private byte addressLow = 0;
        private byte addressHigh = 0;

        private byte tempByte = 0;
        private ushort tempWord = 0;

        private readonly RegisterSet registers;
        private readonly BusInterfaceUnit busInterfaceUnit;

        public ExecutionUnit()
        {
            
        }

        private void on_tick(object sender, EventArgs e)
        {
            tick?.Invoke(sender,e);
        }
    }
}
