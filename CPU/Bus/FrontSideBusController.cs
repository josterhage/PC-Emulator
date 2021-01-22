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
                busCycle = 1;
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
            set
            {
                if (value)
                    while (S02 != BusState.passive || Lock) ;
                _hold = value;
                if (_hold)
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

            busCycle = 0;
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
            if (busCycle < 1)
                return;

            switch (S02)
            {
                case BusState.instructionFetch:
                case BusState.readMemory:
                    if (busCycle == 1)
                        Data = memoryBusController.Read(_address);
                    break;
                case BusState.writeMemory:
                    if (busCycle == 2)
                        memoryBusController.Write(_address, Data);
                    break;
                case BusState.readPort:
                    if (busCycle == 1)
                        Data = ioBusController.Read(_address);
                    break;
                case BusState.writePort:
                    if (busCycle == 2)
                        ioBusController.Write(_address, Data);
                    break;
                default:
                    break;
            }
            if (busCycle < 4)
            {
                busCycle++;
            }
            else
            {
                Data = 0;
                busCycle = -1;
            }
        }
    }
}
