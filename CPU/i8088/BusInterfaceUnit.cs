using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CPU.SystemClock;

namespace CPU.i8088
{
    public partial class Processor
    {
        private partial class BusInterfaceUnit
        {
            private readonly MainTimer mainTimer = MainTimer.GetInstance();

            private event EventHandler TockEvent;

#if DEBUG
            private byte[] memory = new byte[0x100000];
#endif

            private SegmentRegisters segments = new SegmentRegisters();
            private ushort IP = 0;

            private Segment workingSegment;
            private ushort workingOffset = 0;
            private byte tempLow = 0;
            private byte tempHigh = 0;

            private ushort readRequest = 0;
            private Tuple<ushort,byte> writeRequest = new Tuple<ushort,byte>(0,0);

            private ushort temp
            {
                get
                {
                    return (ushort)((tempHigh << 8) | tempLow);
                }
                set
                {
                    tempHigh = (byte)((value & 0xFF00) << 8);
                    tempLow = (byte)(value & 0x00FF);
                }
            }

            private Action onBusCycleComplete;

            private readonly Queue<byte> InstructionQueue = new Queue<byte>(6);

            private BusState s02;

            #region S0-2
            public bool S0
            {
                get
                {
                    return ((int)s02 & 1) != 0;
                }
            }

            public bool S1
            {
                get
                {
                    return ((int)s02 & 2) != 0;
                }
            }

            public bool S2
            {
                get
                {
                    return ((int)s02 & 4) != 0;
                }
            }
            #endregion

            private ReadCycle readCycle = ReadCycle.none;

            private WriteCycle writeCycle = WriteCycle.none;

            private enum BusState
            {
                interruptAcknowledge,
                readPort,
                writePort,
                halt,
                instructionFetch,
                readMemory,
                writeMemory,
                passive
            }

            private enum ReadCycle
            {
                none,
                addressOut,
                statusOut,
                dataIn,
                clear
            }

            private enum WriteCycle
            {
                none,
                addressOut,
                statusOutDataOut1,
                statusOutDataOut2,
                clear
            }

            public BusInterfaceUnit()
            {
                mainTimer.TockEvent += OnTockEvent;
                s02 = BusState.instructionFetch;
            }

            public void OnTockEvent(object sender, EventArgs e)
            {
                switch (s02)
                {
                    case BusState.interruptAcknowledge:
                        throw new NotImplementedException();
                    case BusState.readPort:
                        throw new NotImplementedException();
                    case BusState.writePort:
                        throw new NotImplementedException();
                    case BusState.halt:
                        throw new NotImplementedException();
                    case BusState.instructionFetch:
                        get_instruction();
                        throw new NotImplementedException();
                    case BusState.readMemory:
                        throw new NotImplementedException();
                    case BusState.writeMemory:
                        throw new NotImplementedException();
                    case BusState.passive:
                        throw new NotImplementedException();
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            private void get_instruction()
            {
                workingSegment = Segment.CS;
                workingOffset = IP;
                onBusCycleComplete += got_instruction_handler;
            }

            private void got_instruction_handler()
            {
                InstructionQueue.Enqueue(tempLow);
                IP++;
                if (InstructionQueue.Count == 6)
                {
                    s02 = BusState.passive;
                }
                onBusCycleComplete -= got_instruction_handler;
            }

            private void read_byte()
            {
                //These indicate which cycle just finished, not which cycle we're starting
                switch (readCycle)
                {
                    case ReadCycle.none:
                        //TODO: put address on bus;
                        readCycle = ReadCycle.addressOut;
                        break;
                    case ReadCycle.addressOut:
                        //A16-A19 become S3-S6
                        //AD0-AD7 clear
                        readCycle = ReadCycle.statusOut;
                        break;
                    case ReadCycle.statusOut:
                        //no change?
                        readCycle = ReadCycle.dataIn;
                        break;
                    case ReadCycle.dataIn:
#if DEBUG
                        tempLow = memory[segments[workingSegment] + workingOffset];
#endif
                        readCycle = ReadCycle.clear;
                        break;
                    case ReadCycle.clear:
                        onBusCycleComplete.Invoke();
                        zeroize();
                        readCycle = ReadCycle.none;
                        break;
                }
            }

            private void zeroize()
            {

            }

#if DEBUG
            public void SetMemory(ushort offset, byte value)
            {
                memory[offset] = value;
            }
#endif
        }
    }
}
