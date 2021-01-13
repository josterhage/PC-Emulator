using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SystemBoard.i8088
{
    public class GeneralRegisterChangeEventArgs : EventArgs
    {
        public WordGeneral Register;
        public ushort Value;

        public GeneralRegisterChangeEventArgs(WordGeneral register, ushort value)
        {
            Register = register;
            Value = value;
        }
    }
}
