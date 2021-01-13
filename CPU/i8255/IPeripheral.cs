using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SystemBoard.i8255
{
    public interface IPeripheral
    {
        byte Get();
        void Set(byte data);
    }
}
