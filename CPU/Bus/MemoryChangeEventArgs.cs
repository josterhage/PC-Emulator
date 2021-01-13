using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SystemBoard.Bus
{
    public class MemoryChangeEventArgs : EventArgs
    {
        public byte Value;
        public int Location;

        public MemoryChangeEventArgs(int location, byte value)
        {
            Value = value;
            Location = location;
        }
    }
}
