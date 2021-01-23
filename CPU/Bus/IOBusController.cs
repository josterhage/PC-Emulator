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
            //silently fail
            if (ioMap[port] == null)
                return 0;
            byte value = ioMap[port].Read(port);
            PortReadEvent?.Invoke(this, new PortEventArgs(port, value));
            return value;
        }

        public void Write(int port, byte value)
        {
            //silently fail
            if (ioMap[port] == null)
                return;
            PortWriteEvent?.Invoke(this, new PortEventArgs(port, value));
            ioMap[port].Write(port, value);
        }
    }
}
