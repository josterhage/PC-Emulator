using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SystemBoard.SystemClock;

namespace SystemBoard.i8088
{
    public partial class Processor
    {
        private readonly ExecutionUnit executionUnit;
        private readonly FrontSideBusController fbc;
        private readonly MainTimer mainTimer = MainTimer.GetInstance();

        public event EventHandler<EUChangeEventArgs> EUChangeEvent;
        public event EventHandler<BIUChangeEventArgs> BIUChangeEvent;

        private void on_eu_change(object sender, EUChangeEventArgs e)
        {
            EUChangeEvent?.Invoke(sender, e);
        }

        private void biu_changed()
        {
            BIUChangeEvent?.Invoke(this, new BIUChangeEventArgs(segments, IP));
        }

        private readonly SegmentRegisters segments = new SegmentRegisters();

        private void on_segment_change(object sender, EventArgs e)
        {
            biu_changed();
        }

        private ushort IP
        {
            get => IP;
            set
            {
                IP = value;
                biu_changed();
            }
        }

        private Segment workingSegment;
        private ushort workingOffset = 0;

        private byte tempLow = 0;
        private byte tempHigh = 0;

        private ushort Temp
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

        private byte waitTicks = 0;
        private bool waiting = false;

        private bool handlingInterrupt = false;

        private readonly Queue<byte> InstructionQueue = new Queue<byte>(6);

        public bool InterruptEnabled { get; set; }

        private BusState s02;

        public enum BusState
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

        public bool Test { get; set; } = true;
        public bool Lock { get; private set; } = true;

        private TState tState = TState.none;

        private enum TState
        {
            none,
            address,
            status,
            data,
            clear,
            wait
        }

        //public BusInterfaceUnit(FrontSideBusController fbc)
        //{
        //    bus = fbc;
        //    mainTimer.TockEvent += on_tock_event;
        //    segments.SegmentChangeHandler += on_segment_change;
        //    workingSegment = Segment.CS;
        //    IP = 0;
        //    s02 = BusState.instructionFetch;
        //}

        public byte GetNextFromQueue()
        {
            while (InstructionQueue.Count == 0) ;

            return InstructionQueue.Dequeue();
        }

        #region IO
        public byte InByte(ushort port)
        {
            while (tState != TState.clear || s02 == BusState.halt || handlingInterrupt) ;

            mainTimer.TockEvent -= on_tock_event;

            tState = TState.none;

            workingSegment = Segment.absolute;

            workingOffset = port;

            s02 = BusState.readPort;

            mainTimer.TockEvent += on_tock_event;

            while (tState != TState.clear) ;

            byte result = tempLow;

            s02 = BusState.passive;

            return result;
        }

        public ushort InWord(ushort port)
        {
            ushort result = InByte(port);
            result |= InByte((ushort)(port + 1));
            return result;
        }

        public void OutByte(ushort port, byte value)
        {
            while (tState != TState.clear || s02 == BusState.halt || handlingInterrupt) ;

            mainTimer.TockEvent -= on_tock_event;

            tState = TState.none;

            workingSegment = Segment.absolute;

            workingOffset = port;

            tempLow = value;

            s02 = BusState.writePort;

            mainTimer.TockEvent += on_tock_event;

            while (tState != TState.clear) ;

            s02 = BusState.passive;
        }

        public void OutWord(ushort port, ushort value)
        {
            OutByte(port, (byte)(value & 0xff));
            OutByte((ushort)(port + 1), (byte)((value & 0xff00) >> 8));
        }
        #endregion // IO

        #region MEMRW
        public byte ReadByte(Segment segment, ushort offset)
        {
            while (tState != TState.clear || s02 == BusState.halt || handlingInterrupt) ;

            mainTimer.TockEvent -= on_tock_event;

            tState = TState.none;

            workingSegment = segment;

            workingOffset = offset;

            s02 = BusState.readMemory;

            mainTimer.TockEvent += on_tock_event;

            while (tState != TState.clear) ;

            byte result = tempLow;

            s02 = BusState.passive;

            return result;
        }

        public ushort ReadWord(Segment segment, ushort offset)
        {
            ushort result = ReadByte(segment, offset);
            result |= ReadByte(segment, (ushort)(offset + 1));
            return result;
        }

        public void WriteByte(Segment segment, ushort offset, byte value)
        {
            while (tState != TState.clear || s02 == BusState.halt || handlingInterrupt) ;

            mainTimer.TockEvent -= on_tock_event;

            tState = TState.none;

            workingSegment = segment;

            workingOffset = offset;

            tempLow = value;

            s02 = BusState.writeMemory;

            mainTimer.TockEvent += on_tock_event;

            while (tState != TState.clear) ;

            s02 = BusState.passive;
        }

        public void WriteWord(Segment segment, ushort offset, ushort value)
        {
            WriteByte(segment, offset, (byte)(value & 0xff));
            WriteByte(segment, (ushort)(offset + 1), (byte)((value & 0xff00) >> 8));
        }

        public void WriteSegmentToMemory(Segment segment, ushort offset, Segment segmentOverride = Segment.DS)
        {
            WriteWord(segmentOverride, offset, segments[segment]);
        }

        public void WriteIPToStack(ushort offset)
        {
            WriteWord(Segment.SS, offset, IP);
        }

        public void SetSegment(Segment segment, ushort value)
        {
            segments[segment] = value;
        }

        public ushort GetSegment(Segment segment)
        {
            return segments[segment];
        }
        #endregion MEMRW

        #region BUSCONTROL
        public void AssertLock()
        {
            Lock = false;
        }

        public void DeassertLock()
        {
            Lock = true;
        }

        public void Wait()
        {
            if (!Test)
            {
                mainTimer.TockEvent -= on_tock_event;
                tState = TState.none;
                s02 = BusState.passive;
                waitTicks = 0;
                mainTimer.TockEvent += wait_handler;
                waiting = true;

                while (waiting) ;
            }
        }

        public void Halt()
        {
            while (tState != TState.clear || s02 == BusState.halt || handlingInterrupt) ;

            s02 = BusState.halt;
        }

        private void wait_handler(object sender, EventArgs e)
        {
            waitTicks++;
            if (waitTicks == 5)
            {
                if (Test)
                {
                    waiting = false;
                    mainTimer.TockEvent -= wait_handler;
                    if (InstructionQueue.Count < 6)
                    {
                        tState = TState.none;
                        s02 = BusState.instructionFetch;
                    }
                    mainTimer.TockEvent += on_tock_event;
                }
                else
                {
                    waitTicks = 0;
                }
            }
        }
        #endregion BUSCONTROL

        #region EVENTHANDLERS
        private void single_cycle_write_handler(object sender, EventArgs e)
        {
            tState = TState.none;
            mainTimer.TockEvent -= single_cycle_write_handler;
        }

        private void on_tock_event(object sender, EventArgs e)
        {
            //this should be set NLT T4 on each read/write cycle
            switch (s02)
            {
                case BusState.interruptAcknowledge:
                    throw new NotImplementedException();
                case BusState.readPort:
                    read_byte();
                    break;
                case BusState.writePort:
                    write_byte();
                    break;
                case BusState.halt:
                    break;
                case BusState.instructionFetch:
                    get_instruction();
                    break;
                case BusState.readMemory:
                    read_byte();
                    break;
                case BusState.writeMemory:
                    write_byte();
                    break;
                case BusState.passive:
                    if (InstructionQueue.Count < 6)
                    {
                        tState = TState.none;
                        s02 = BusState.instructionFetch;
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        #endregion EVENTHANDLERS

        private void get_instruction()
        {
            if (tState == TState.none)
            {
                workingSegment = Segment.CS;
                workingOffset = IP;
            }
            else if (tState == TState.clear)
            {
                InstructionQueue.Enqueue(tempLow);
                IP++;

                if (InstructionQueue.Count == 6 && s02 == BusState.instructionFetch)
                {
                    s02 = BusState.passive;
                }
            }
            read_byte();
        }

        private void read_byte()
        {
            //These indicate which cycle just finished, not which cycle we're starting
            switch (tState)
            {
                case TState.none: //begin T1
                                  //TODO: put address on bus;
                    fbc.Address = (segments[workingSegment] << 4) + workingOffset;
                    tState = TState.address;
                    break;
                case TState.address: //begin T2
                                     //A16-A19 become S3-S6
                                     //AD0-AD7 clear
                    fbc.S34 = (byte)workingSegment;
                    fbc.S5 = InterruptEnabled;
                    tState = TState.status;
                    break;
                case TState.status: //begin T3
                                    //no change?
                    tState = TState.data;
                    break;
                case TState.data: //begin T4
                    tempLow = fbc.Data;

                    //#if DEBUG
                    //                        tempLow = memory[(segments[workingSegment] << 4) + workingOffset];
                    //#endif
                    tState = TState.clear;
                    break;
                case TState.wait:
                    // does the BIU need to do anything here? ensure that a pin is set or something?
                    break;
                case TState.clear:
                    //onBusCycleComplete.Invoke();
                    tState = TState.none;
                    break;
            }
        }

        private void write_byte()
        {
            switch (tState)
            {
                case TState.none://begin T1
                    fbc.Address = (segments[workingSegment] << 4) + workingOffset;
                    tState = TState.address;
                    break;
                case TState.address: //begin T2
                    fbc.S34 = (byte)workingSegment;
                    fbc.S5 = InterruptEnabled;
                    fbc.Data = tempLow;
                    tState = TState.status;
                    break;
                case TState.status: //begin T3
                    tState = TState.data;
                    break;
                case TState.data: //begin T4
                    tState = TState.clear;
                    //#if DEBUG
                    //                        memory[(segments[workingSegment] << 4) + workingOffset] = tempLow;
                    //#endif
                    break;
                case TState.wait:
                    break;
                case TState.clear:
                    tState = TState.none;
                    break;
            }
        }

        #region JUMPS
        public void JumpNear(ushort offset)
        {
            mainTimer.TockEvent -= on_tock_event;

            tState = TState.none;

            s02 = BusState.instructionFetch;

            if ((offset & 0x8000) != 0)
            {
                offset ^= 0xffff;
                offset++;
                IP -= offset;
            }
            else
            {
                IP += offset;
            }

            InstructionQueue.Clear();

            mainTimer.TockEvent += on_tock_event;
        }

        public void JumpFar(ushort newCS, ushort newIP)
        {
            mainTimer.TockEvent -= on_tock_event;

            s02 = BusState.instructionFetch;

            segments.CS = newCS;

            IP = newIP;

            InstructionQueue.Clear();

            mainTimer.TockEvent += on_tock_event;
        }

        public void JumpImmediate(ushort newIP)
        {
            mainTimer.TockEvent -= on_tock_event;

            s02 = BusState.instructionFetch;

            IP = newIP;

            InstructionQueue.Clear();

            mainTimer.TockEvent += on_tock_event;
        }

        public void JumpToInterruptVector(ushort interrupt)
        {
            ushort ip = ReadWord(Segment.absolute, (ushort)(interrupt * 4));

            ushort cs = ReadWord(Segment.absolute, (ushort)((interrupt * 4) + 2));

            mainTimer.TockEvent -= on_tock_event;

            s02 = BusState.instructionFetch;

            IP = ip;

            segments.CS = cs;

            InstructionQueue.Clear();

            mainTimer.TockEvent += on_tock_event;
        }
        #endregion //JUMPS

        public void JumpShort(byte offset)
        {
            mainTimer.TockEvent -= on_tock_event;

            tState = TState.none;

            s02 = BusState.instructionFetch;

            if ((offset & 0x80) != 0)
            {
                offset ^= 0xff;
                offset++;
                IP -= offset;
            }
            else
            {
                IP += offset;
            }

            InstructionQueue.Clear();

            mainTimer.TockEvent += on_tock_event;
        }

        public Processor(FrontSideBusController fbc)
        {   segments.SegmentChangeHandler += on_segment_change;
            executionUnit = new ExecutionUnit(this);
            executionUnit.EUChangedEvent += on_eu_change;
            this.fbc = fbc;
            executionUnit.Run();
        }
    }
}
