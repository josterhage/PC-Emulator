using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SystemBoard.Bus
{
    public class IOBusController
    {
        public event EventHandler<PortEventArgs> PortReadEvent;
        public event EventHandler<PortEventArgs> PortWriteEvent;

        private readonly IMemoryLocation[] ioMap = new IMemoryLocation[65536];

        public IOBusController() { }

        public void Register(IMemoryLocation port)
        {
            for(int i = port.BaseAddress; i < port.BaseAddress + port.Size; i++)
            {
                ioMap[i] = port;
            }
        }

        public byte Read(int port)
        {
            if (ioMap[port] == null)
                throw new InvalidOperationException();
            byte value = ioMap[port].Read(port);
            PortReadEvent?.Invoke(this, new PortEventArgs(port, value));
            return value;
        }

        public void Write(int port, byte value)
        {
            if (ioMap[port] == null)
                throw new InvalidOperationException();
            PortWriteEvent?.Invoke(this, new PortEventArgs(port, value));
            ioMap[port].Write(port, value);
        }
    }
}
