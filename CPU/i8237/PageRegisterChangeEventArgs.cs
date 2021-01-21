using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SystemBoard.i8237
{
    public class PageRegisterChangeEventArgs :EventArgs
    {
        public byte Page { get; set; }
        public byte Value { get; set; }

        public PageRegisterChangeEventArgs(byte page, byte value)
        {
            Page = page;
            Value = value;
        }
            
    }
}
