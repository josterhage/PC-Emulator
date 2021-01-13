using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SystemBoard.Bus
{
    public interface IMemoryLocation
    {
        int Size { get; }
        int BaseAddress { get; }

        void Write(int location, byte value);
        byte Read(int location);
    }
}
