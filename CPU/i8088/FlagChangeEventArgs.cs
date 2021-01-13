using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SystemBoard.i8088
{
    public class FlagChangeEventArgs : EventArgs
    {
        public FlagRegister Flags;

        public FlagChangeEventArgs(FlagRegister flags)
        {
            Flags = flags;
        }
    }
}
