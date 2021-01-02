using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SystemBoard.i8088;

namespace SystemBoard.Bus
{
#if DEBUG
    public class FrontSideBusController
        #else
    internal class FrontSideBusController
#endif
    {
        private readonly MemoryBusController memoryBusController;
        private readonly Processor cpu;

        public int Address
        {
            get => Address;
            set
            {
                switch (S02)
                {
                    case BusState.instructionFetch:
                    case BusState.readMemory:
                    case BusState.readPort:
                        // read data
                        break;
                    case BusState.writeMemory:
                    case BusState.writePort:
                        // write data
                        break;
                    default:
                        break;
                }
            }
        }
        public byte Data { get; set; }
        public BusState S02 { get; set; }
        public Segment S34 { get; set; }
        public bool S5 { get; set; }

        public FrontSideBusController(MemoryBusController memoryBusController, Processor cpu)
        {
            this.memoryBusController = memoryBusController;
            this.cpu = cpu;
        }
    }
}
