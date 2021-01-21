using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SystemBoard.Bus;
using SystemBoard.SystemClock;

namespace SystemBoard.i8237
{
    public class DmaController : IMemoryLocation
    {
        private readonly MainTimer timer = MainTimer.GetInstance();
        private FrontSideBusController frontSideBusController;

        private int cycleState;

        // overhead required to register with the IOController
        private const int _size = 16;
        public int Size => _size;

        private const int _baseAddress = 0;
        public int BaseAddress => _baseAddress;

        private bool _hlda = false;
        public bool Hlda
        {
            set
            {
                _hlda = value;
            }
        }

        public bool DREQ0 { private get; set; }
        public bool DACK0 { get; private set; }

        public bool DREQ1 { private get; set; }
        public bool DACK1 { get; private set; }

        public bool DREQ2 { private get; set; }
        public bool DACK2 { get; private set; }

        public bool DREQ3 { private get; set; }
        public bool DACK3 { get; private set; }

        private int activeDreq;

        // the pages are the MSBs required for each DMA channel to address 1mb
        // stored at 0x80-0x83, the PageRegister class triggers an event when their value changes
        private readonly byte[] pages = new byte[4];

        // flipflop 0 is write, flipflop 1 is read
        private readonly bool[,] commandFlipFlops = new bool[8, 2];

        // 0,1 - channel 0 address, word
        // 2,3 - channel 1 address, word
        // 4,5 - channel 2 address, word
        // 6,7 - channel 3 address, word
        private readonly ushort[] baseRegisters = new ushort[8];
        private readonly ushort[] currentRegisters = new ushort[8];

        private readonly bool[] requests = new bool[4];

        private readonly byte[] modes = new byte[4];

        private readonly bool[] masks = new bool[4];

        private ushort tempAddress;
        private ushort tempWord;

        private byte status;
        private byte command;
        private byte temp;

        //bit masks
        private const byte transferMode = 0xc0;
        private const byte transferIncrementDecrement = 0x20;
        private const byte autoInit = 0x10;
        private const byte verWriteRead = 0x0c;
        private const byte channel = 0x03;

        public byte Read(int location)
        {
            if (cycleState > 0)
                return 0;

            var port = location - _baseAddress;

            byte result;

            // 0x00-0x07 -- count/address registers
            if (port < 8)
            {
                //get the byte based on the flip-flop value
                result = commandFlipFlops[port, 1] ? (byte)((currentRegisters[port] & 0xff00) >> 8) : (byte)(currentRegisters[port] & 0xff);
                // flip the flip-flop
                commandFlipFlops[port, 1] ^= true;
            }
            else if (port == 8)
            {
                //status register
                result = status;
            }
            else if (port == 13)
            {
                //temporary register
                result = temp;
            }
            else
                throw new ArgumentOutOfRangeException();

            return result;
        }

        public void Write(int location, byte value)
        {
            if (cycleState > 0)
                return;

            var port = location - _baseAddress;

            // 0x00-0x07 -- count/address registers
            if (port < 8)
            {
                baseRegisters[port] = commandFlipFlops[port, 0] ? (ushort)(baseRegisters[port] | (value << 8)) : value;
                currentRegisters[port] = commandFlipFlops[port, 0] ? (ushort)(currentRegisters[port] | (value << 8)) : value;
            }
            else
            {
                var channel = value & 3;
                switch (port)
                {
                    case 8:
                        command = value;
                        break;
                    case 9:
                        requests[channel] = (value & 4) != 0;
                        break;
                    case 10:
                        masks[channel] = (value & 4) != 0;
                        break;
                    case 11:
                        modes[channel] = value;
                        break;
                    case 12:
                        //the datasheet isn't perfectly clear, but i'm going to assume this is how this works
                        commandFlipFlops[channel * 2, 0] = false;
                        commandFlipFlops[channel * 2, 1] = false;
                        commandFlipFlops[(channel * 2) + 1, 0] = false;
                        commandFlipFlops[(channel * 2) + 1, 1] = false;
                        break;
                    case 13:
                        command = 0;
                        temp = 0;
                        tempAddress = 0;
                        tempWord = 0;
                        for (int i = 0; i < 4; i++)
                        {
                            requests[i] = false;
                            masks[i] = false;
                            modes[i] = 0;
                            baseRegisters[i] = 0;
                            baseRegisters[i + 4] = 0;
                            currentRegisters[i] = 0;
                            currentRegisters[i + 4] = 0;
                            for (int j = 0; j < 4; j++)
                            {
                                commandFlipFlops[(i * 4) + j, 0] = false;
                                commandFlipFlops[(i * 4) + j, 1] = false;
                            }
                        }
                        break;
                    case 14:
                        for (int i = 0; i < 4; i++)
                        {
                            masks[i] = false;
                        }
                        break;
                    case 15:
                        for (int i = 0; i < 4; i++)
                        {
                            masks[i] = (byte)(value & (1 << i)) != 0;
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public DmaController() { }

        public void RegisterFrontSideBusController(FrontSideBusController frontSideBusController)
        {
            this.frontSideBusController = frontSideBusController;
        }

        public void RegisterPageRegisterChangeEvent(PageRegister p)
        {
            p.PageRegisterChange += OnPageRegisterChangeEvent;
        }

        protected void OnPageRegisterChangeEvent(object sender, PageRegisterChangeEventArgs e)
        {
            pages[e.Page] = e.Value;
        }

        private void on_tock(object sender, TimerEventArgs e)
        {
            switch (cycleState)
            {
                case 0:
                    //poll the DRQ lines
                    activeDreq = poll_drq();
                    if (activeDreq < 0)
                        return;

                    frontSideBusController.Hold = true;

                    while (!_hlda) ;

                    cycleState = 1;
                    break;
                case 1:
                    // functional priority: single, block, demand, cascade
                    switch ((modes[activeDreq] & transferMode) >> 6)
                    {
                        //demand transfer mode
                        case 0:
                            break;
                        //single transfer mode
                        case 1:
                            break;
                        //block transfer mode
                        case 2:
                            break;
                        // cascade mode
                        case 3:
                            break;
                        default:
                            throw new InvalidOperationException();
                    }
                    break;
            }
        }

        private int poll_drq()
        {
            if (DREQ0)
                return 0;
            if (DREQ1)
                return 1;
            if (DREQ2)
                return 2;
            if (DREQ3)
                return 3;
            return -1;
        }
    }
}
