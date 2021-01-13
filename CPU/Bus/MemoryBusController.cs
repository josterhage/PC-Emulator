using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SystemBoard.Bus
{
    public class MemoryBusController
    {
        public event EventHandler<MemoryChangeEventArgs> MemoryChangeEvent;

        private readonly IMemoryLocation[] memoryMap = new IMemoryLocation[1048576];

        public MemoryBusController() { }

        public void Register(IMemoryLocation location)
        {
            for (int i = location.BaseAddress; i < location.BaseAddress + location.Size; i++)
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
            MemoryChangeEvent?.Invoke(this, new MemoryChangeEventArgs(location, value));
        }

        internal byte[] GetMemoryMap()
        {
            byte[] values = new byte[1048576];
            for(int i =0; i < 1048576; i++)
            {
                if(memoryMap[i] != null)
                {
                    values[i] = memoryMap[i].Read(i);
                }
                else
                {
                    values[i] = 0;
                }
            }

            return values;
        }

        internal byte[] GetSegmentMap(int baseAddress)
        {
            byte[] values = new byte[65536];
            for (int i = 0; i < 65536; i++)
            {
                if (baseAddress + i > 1048575)
                    return values;
                if(memoryMap[baseAddress + i] != null)
                {
                    values[i] = memoryMap[baseAddress + i].Read(baseAddress + i);
                }
                else
                {
                    values[i] = 0;
                }
            }
            return values;
        }
    }
}
