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
            private enum ModEncoding
            {
                registerDisplacement,
                byteDisplacement,
                wordDisplacement,
                registerRegister
            }

            private enum RmEncoding
            {
                BXSI, //0b000
                BXDI, //0b001
                BPSI, //0b010
                BPDI, //0b011
                SI,   //0b100
                DI,   //0b101
                BP,   //0b110
                BX    //0b111
            }
        }
    }
}
