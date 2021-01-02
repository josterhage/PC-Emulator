using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SystemBoard
{
    public interface IMemoryLocation
    {
        int BaseAddress { get; }

        void Write(int location, byte value);
        byte Read(int location);
    }
}
