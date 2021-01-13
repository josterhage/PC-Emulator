using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SystemBoard.i8088;
using SystemBoard.i8259;
using SystemBoard.SystemClock;

namespace SystemBoard.Bus
{
#if DEBUG
    public class FrontSideBusController
        #else
    internal class FrontSideBusController
#endif
    {
        private readonly MemoryBusController memoryBusController;
        private readonly IOBusController ioBusController;
        private readonly InterruptController interruptController;
        private readonly MainTimer timer = MainTimer.GetInstance();

        private delegate void Write();
        //private readonly Processor cpu;
        private int _address;

        public int Address
        {
            get => _address;
            set
            {
                _address = value;
                switch (S02)
                {
                    case BusState.instructionFetch:
                    case BusState.readMemory:
                        Data = memoryBusController.Read(_address);
                        break;
                    case BusState.writeMemory:
                        memoryBusController.Write(_address, Data);
                        break;
                    case BusState.readPort:
                        Data = ioBusController.Read(_address);
                        break;
                    case BusState.writePort:
                        ioBusController.Write(_address, Data);
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

        public FrontSideBusController(MemoryBusController memoryBusController, IOBusController ioBusController)
        {
            this.memoryBusController = memoryBusController;
            this.ioBusController = ioBusController;
        }

        private void wait(int clock_cycles)
        {
            var ticks = timer.TotalTicks;
            while ((timer.TotalTicks - ticks) < (ulong)clock_cycles) ;
        }
    }
}
