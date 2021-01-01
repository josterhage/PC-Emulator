using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SystemBoard.i8088
{
    public class EUChangeEventArgs : EventArgs
    {
        public GeneralRegisters Registers;
        public FlagRegister Flags;
        public byte Opcode;

        public EUChangeEventArgs(GeneralRegisters g, FlagRegister f, byte opcode)
        {
            Registers = g;
            Flags = f;
            Opcode = opcode;
        }
    }
}
