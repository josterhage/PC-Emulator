using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SystemBoard.i8088;

namespace SystemBoard
{
#if DEBUG
    public class FrontSideBusController
        #else
    internal class FrontSideBusController
#endif
    {
        //private readonly MemoryBusController memoryBus;
        //private readonly IOBusController ioBus;
        private readonly i8088.Processor.BusInterfaceUnit biu;

        public int Address { get; set; }
        public byte Data { get; set; }
        public byte S02 { get; set; }
        public byte S34 { get; set; }
        public bool S5 { get; set; }
    }
}
