using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SystemBoard.Bus
{
    public class MemoryBusController
    {
        private readonly IMemoryLocation[] memoryMap = new IMemoryLocation[1048576];

        public MemoryBusController() { }

        public void Register(IMemoryLocation location, int offset, int size)
        {
            for (int i = offset; i < offset + size; i++)
            {
                memoryMap[i] = location;
            }
        }

        public byte Read(int location)
        {
            //TODO: make this generate an NMI
            if (memoryMap[location] == null)
                throw new InvalidOperationException();
            return memoryMap[location].Read(location);
        }

        public void Write(int location, byte value)
        {
            //TODO: make this generate an NMI
            if (memoryMap[location] == null)
                throw new InvalidOperationException();
            memoryMap[location].Write(location, value);
        }
    }
}
