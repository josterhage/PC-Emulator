using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SystemBoard.Bus;

namespace SystemBoard.i8237
{
    public class PageRegister : IMemoryLocation
    {
        public event EventHandler<PageRegisterChangeEventArgs> PageRegisterChange;

        private readonly byte[] pages = new byte[4];

        private const int _size = 4;
        public int Size => _size;

        private const int _baseAddress = 0x80;
        public int BaseAddress => _baseAddress;

        public byte Read(int location)
        {
            return pages[location - _baseAddress];
        }

        public void Write(int location, byte value)
        {
            pages[location - _baseAddress] = (byte)(value & 0x0f);

            PageRegisterChange?.Invoke(this, new PageRegisterChangeEventArgs((byte)(location - _baseAddress), value));
        }
    }
}
