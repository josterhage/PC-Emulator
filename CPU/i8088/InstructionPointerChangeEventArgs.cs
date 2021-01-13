using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SystemBoard.i8088
{
    public class InstructionPointerChangeEventArgs : EventArgs
    {
        public int IP;

        public InstructionPointerChangeEventArgs(int IP)
        {
            this.IP = IP;
        }
    }
}
