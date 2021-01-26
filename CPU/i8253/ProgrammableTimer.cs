using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SystemBoard.Bus;
using SystemBoard.SystemClock;

namespace SystemBoard.i8253
{
    public class ProgrammableTimer : IMemoryLocation
    {
        private readonly MainTimer timer = MainTimer.GetInstance();

        private const byte selectMask = 0xc0;
        private const byte readLoadMask = 0x30;
        private const byte modeMask = 0x0e;
        private const byte bcdMask = 1;

        private const int _size = 4;
        public int Size => _size;

        private const int _baseAddress = 0x40;
        public int BaseAddress => _baseAddress;

        private ushort countLatch;
        private bool countLatchFlipFlop;

        private readonly byte[] controlWords = new byte[3];

        private readonly ushort[] counters = new ushort[3];
        private readonly ushort[] currentCounters = new ushort[3];
        private readonly bool[,] readWriteFlipFlops = new bool[3, 2];
        private readonly bool[] running = new bool[3];

        private bool[] _gates = new bool[3];

        public bool GATE0
        {
            set
            {
                gate_change(0);
                _gates[0] = value;
            }
        }

        public bool GATE1
        {
            set
            {
                gate_change(1);
                _gates[1] = value;
            }
        }

        public bool GATE2
        {
            set
            {
                gate_change(2);
                _gates[2] = value;
            }
        }

        public bool OUT0 { get; private set; }
        public bool OUT1 { get; private set; }
        public bool OUT2 { get; private set; }

        public byte Read(int location)
        {
            var port = location - _baseAddress;
            byte result;
            if (port < 3)
            {
                var readLoad = (byte)((controlWords[port] & readLoadMask) >> 4);
                switch (readLoad)
                {
                    case 0:
                        result = countLatchFlipFlop ? (byte)((countLatch & 0xff00) >> 8) : (byte)(countLatch & 0xff);
                        countLatchFlipFlop ^= true;
                        break;
                    case 1:
                        result = (byte)(counters[port] & 0xff); 
                        break;
                    case 2:
                        result = (byte)((counters[port] & 0xff00) >> 8);
                        break;
                    case 3:
                        result = readWriteFlipFlops[port, 0] ? (byte)((counters[port] & 0xff00) >> 8) : (byte)(counters[port] & 0xff);
                        readWriteFlipFlops[port, 0] ^= true;
                        break;
                    default:
                        throw new InvalidOperationException();
                }
            }
            else
            {
                result = 0;
            }
            return result;
        }

        public void Write(int location, byte value)
        {
            var port = location - _baseAddress;
            byte readload;

            if (port < 3)
            {
                readload = (byte)((controlWords[port] & readLoadMask) >> 4);
                var mode = (byte)((controlWords[port] & modeMask) >> 1);
                
                //if this counter is set to BCD mode, truncate values as necessary
                if ((controlWords[port] & bcdMask) != 0)
                {
                    if ((value & 0xf0) > 0x90)
                        value &= 0x9f;
                    if ((value & 0x0f) > 0x09)
                        value &= 0xf9;
                }

                if (running[port] & (mode == 0))
                    running[port] = false;

                switch (readload)
                {
                    case 0:
                        //invalid;
                        return;
                    case 1:                       
                        counters[port] &= 0xff00;
                        counters[port] |= value;
                        if (mode == 0)
                            running[port] = _gates[port];
                        break;
                    case 2:
                        counters[port] &= 0x00ff;
                        counters[port] |= (ushort)(value << 8);
                        if (mode == 0)
                            running[port] = _gates[port];
                        break;
                    case 3:
                        counters[port] = readWriteFlipFlops[port, 1] ? (ushort)(counters[port] | (value << 8)) : value;
                        readWriteFlipFlops[port, 1] ^= true;
                        if (mode == 0 && !readWriteFlipFlops[port,1])
                            running[port] = _gates[port];
                        break;
                    default:
                        throw new InvalidOperationException();
                }
            }
            else if (port == 3)
            {
                //write mode word
                var counter = (byte)((value & selectMask) >> 6);
                controlWords[counter] = value;
                readload = (byte)((value & readLoadMask) >> 4);
                if(readload == 0)
                {
                    countLatch = counters[counter];
                }
            }
            else
                throw new ArgumentOutOfRangeException();
        }

        public ProgrammableTimer()
        {
            timer.TockEvent += OnTockEvent;
        }

        protected void OnTockEvent(object sender, TimerEventArgs e)
        {
            //clear all outputs
            OUT0 = OUT1 = OUT2 = false;
            for(int i = 0; i < 3; i++)
            {
                if(counters[i] > 0)
                {
                    if((controlWords[i] & 1) != 0)
                    {
                        counters[i] = decrement_bcd_word(counters[i]);
                    }
                    else
                    {
                        counters[i]--;
                    }
                }
                
            }
        }

        private ushort decrement_bcd_word(ushort word)
        {
            if (word == 0)
                return word;

            if((word&0xf) == 0)
            {
                if((word&0xf0) == 0)
                {
                    if((word&0xf00) == 0){
                        word -= 0x1000;
                    }
                    else
                    {
                        word -= 0x100;
                    }
                }
                else
                {
                    word -= 0x10;
                }
            }
            else
            {
                word--;
            }
            return word;
        }

        private void gate_change(int gate)
        {
            var mode = (byte)((controlWords[gate] & modeMask) >> 1);

            if(mode == 0)
            {
                running[gate] = _gates[gate];
                return;
            }

            switch (gate)
            {
                case 0:
                    
                    break;
                case 1:
                    break;
                case 2:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
