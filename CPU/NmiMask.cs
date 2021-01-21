using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SystemBoard.Bus;

namespace SystemBoard
{
    /// <summary>
    /// This emulator doesn't simulat Non-Maskable Interrupts, so this is a bit-sink for BIOS routines that expect to be able to mask NMIs
    /// </summary>
    public class NmiMask : IMemoryLocation
    {
        private byte value;

        private const int _size = 1;
        public int Size => _size;

        private const int _baseAddress = 0xa0;
        public int BaseAddress => _baseAddress;

        public byte Read(int location)
        {
            return value;
        }

        public void Write(int location, byte value)
        {
            this.value = value;
        }
    }
}
