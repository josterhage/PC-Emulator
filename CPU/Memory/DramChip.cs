using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SystemBoard.Memory
{
    public class DramChip : IMemoryLocation
    {
        private int _baseAddress;
        public int BaseAddress
        {
            get => _baseAddress;
        }

        private byte[] data = new byte[16384];

        public DramChip(int baseAddress)
        {
            _baseAddress = baseAddress;
        }


        public byte Read(int location)
        {
            return data[location - _baseAddress];
        }

        public void Write(int location, byte value)
        {
            data[location - _baseAddress] = value;
        }
    }
}
