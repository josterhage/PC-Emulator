using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SystemBoard.i8088;
using SystemBoard.i8237;
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
        private readonly MainTimer timer = MainTimer.GetInstance();
        private readonly DmaController dmaController;

        private delegate void Write();
        //private readonly Processor cpu;
        private int _address;

        public int Address
        {
            get => _address;
            set
            {
                _address = value;
                busCycle = 0;
            }
        }
        public byte Data { get; set; }
        public BusState S02 { get; set; }
        public Segment S34 { get; set; }
        public bool S5 { get; set; }
        public bool Lock { get; set; }

        private bool _hold;
        public bool Hold
        {
            get => _hold;
            set {
                if (value)
                    while (S02 != BusState.passive || Lock) ;
                _hold = value;
                if(_hold)
                    do_hold();
            }
        }

        private int busCycle;

        public FrontSideBusController(MemoryBusController memoryBusController, IOBusController ioBusController, DmaController dmaController)
        {
            this.memoryBusController = memoryBusController;
            this.ioBusController = ioBusController;
            this.dmaController = dmaController;

            dmaController.RegisterFrontSideBusController(this);

            busCycle = -1;
            timer.TockEvent += OnTockEvent;
        }

        private void do_hold()
        {
            dmaController.Hlda = true;
            while (_hold) ;
            dmaController.Hlda = false;
        }

        protected void OnTockEvent(object sender, TimerEventArgs e)
        {
            if (busCycle < 0)
                return;

            switch (S02)
            {
                case BusState.instructionFetch:
                case BusState.readMemory:
                    if (busCycle == 0)
                        Data = memoryBusController.Read(_address);
                    else if (busCycle == 2)
                        Data = 0;
                    break;
                case BusState.writeMemory:
                    if (busCycle == 0)
                        memoryBusController.Write(_address, Data);
                    else if (busCycle == 2)
                        Data = 0;
                    break;
                case BusState.readPort:
                    if (busCycle == 0)
                        Data = ioBusController.Read(_address);
                    else if (busCycle == 2)
                        Data = 0;
                    break;
                case BusState.writePort:
                    if (busCycle == 0)
                        ioBusController.Write(_address, Data);
                    else if (busCycle == 2)
                        Data = 0;
                    break;
                default:
                    break;
            }
            if (busCycle < 2)
                busCycle++;
            else
                busCycle = -1;
        }
    }
}
