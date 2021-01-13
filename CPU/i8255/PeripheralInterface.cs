using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SystemBoard.Bus;

namespace SystemBoard.i8255
{
    public class PeripheralInterface : IMemoryLocation
    {
        private readonly int _baseAddress;

        private readonly IPeripheral[] peripherals = new IPeripheral[3];

        public PeripheralInterface(int baseAddress)
        {
            _baseAddress = baseAddress;
        }

        public int Size => 4;

        public int BaseAddress => _baseAddress;

        public byte Read(int location)
        {
            if (location - _baseAddress == 4)
            {
                // configuration
                return 0;
            }
            else return peripherals[location - _baseAddress].Get();
        }

        public void Write(int location, byte value)
        {
            if(location - _baseAddress == 4)
            {
                //set configuration
            }
            else
            {
                peripherals[location - _baseAddress].Set(value);
            }
        }
    }
}
