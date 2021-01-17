using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SystemBoard.Bus;
using SystemBoard.SystemClock;
using SystemBoard.i8259;

namespace SystemBoard.i8088
{
    public class Processor 
    {
        private readonly ExecutionUnit executionUnit;
        private readonly FrontSideBusController fbc;
        private readonly MainTimer mainTimer = MainTimer.GetInstance();
        private readonly SegmentRegisters segments = new SegmentRegisters();

        private ushort _ip;

        private ushort IP
        {
            get => _ip;
            set
            {
                _ip = value;
                InstructionPointerChangeEvent?.Invoke(this, new InstructionPointerChangeEventArgs(_ip));
            }
        }

        private Segment workingSegment;
        private ushort workingOffset = 0;

        private byte temp = 0;

        private readonly Queue<byte> InstructionQueue = new Queue<byte>(6);

        public event EventHandler<GeneralRegisterChangeEventArgs> GeneralRegisterChangeEvent;
        public event EventHandler<FlagChangeEventArgs> FlagRegisterChangeEvent;
        public event EventHandler<SegmentChangeEventArgs> SegmentChangeEvent;
        public event EventHandler<InstructionPointerChangeEventArgs> InstructionPointerChangeEvent;

        private byte waitTicks = 0;
        private bool waiting = false;

        private bool handlingInterrupt = false;

        public bool InterruptEnabled { get; set; }

        private BusState _s02;

        private BusState S02
        {
            get => _s02;
            set
            {
                _s02 = value;
                fbc.S02 = _s02;
            }
        }

        public bool Test { get; set; } = true;
        public bool Lock { get; private set; } = true;

        public InterruptController InterruptController { get; internal set; }
        
        public bool INTR { get; internal set; }
        
        public bool Nmi { get; internal set; } = false;

        private TState tState = TState.none;

        private TState nextState = TState.address;

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
            while (InstructionQueue.Count == 0 && mainTimer.IsRunning) ;
            if (!mainTimer.IsRunning)
                return 0;
            return InstructionQueue.Dequeue();
        }

        #region IO
        public byte InByte(ushort port)
        {
            while ((tState != TState.clear || S02 == BusState.halt || handlingInterrupt) && mainTimer.IsRunning) ;

            mainTimer.TockEvent -= on_tock_event;

            nextState = TState.none;

            workingSegment = Segment.IO;

            workingOffset = port;

            S02 = BusState.readPort;

            mainTimer.TockEvent += on_tock_event;

            while (nextState != TState.clear) ;

            byte result = temp;

            S02 = BusState.passive;

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
            while ((tState != TState.clear || S02 == BusState.halt || handlingInterrupt) && mainTimer.IsRunning) ;

            mainTimer.TockEvent -= on_tock_event;

            nextState = TState.none;

            workingSegment = Segment.IO;

            workingOffset = port;

            temp = value;

            S02 = BusState.writePort;

            mainTimer.TockEvent += on_tock_event;

            while (nextState != TState.clear) ;

            S02 = BusState.passive;
        }

        internal byte Inta()
        {
            S02 = BusState.interruptAcknowledge;
            while (S02 == BusState.interruptAcknowledge) ;
            return temp;
        }

        public void OutWord(ushort port, ushort value)
        {
            OutByte(port, (byte)(value & 0xff));
            OutByte((ushort)(port + 1), (byte)((value & 0xff00) >> 8));
        }
        #endregion IO

        #region MEMRW
        public byte ReadByte(Segment segment, ushort offset)
        {
            while ((tState != TState.clear || S02 == BusState.halt || handlingInterrupt) && mainTimer.IsRunning) ;

            mainTimer.TockEvent -= on_tock_event;

            nextState = TState.none;

            workingSegment = segment;

            workingOffset = offset;

            S02 = BusState.readMemory;

            mainTimer.TockEvent += on_tock_event;

            while (nextState != TState.clear) ;

            byte result = temp;

            S02 = BusState.passive;

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
            while ((tState != TState.clear || S02 == BusState.halt || handlingInterrupt) && mainTimer.IsRunning) ;

            mainTimer.TockEvent -= on_tock_event;

            nextState = TState.none;

            workingSegment = segment;

            workingOffset = offset;

            temp = value;

            S02 = BusState.writeMemory;

            mainTimer.TockEvent += on_tock_event;

            while (nextState != TState.clear) ;

            S02 = BusState.passive;
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
                nextState = TState.none;
                S02 = BusState.passive;
                waitTicks = 0;
                mainTimer.TockEvent += wait_handler;
                waiting = true;

                while (waiting) ;
            }
        }

        public void Halt()
        {
            S02 = BusState.halt;

            while ((tState != TState.clear || S02 == BusState.halt || handlingInterrupt) && mainTimer.IsRunning) ;
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
                        nextState = TState.none;
                        S02 = BusState.instructionFetch;
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
            nextState = TState.none;
            mainTimer.TockEvent -= single_cycle_write_handler;
        }

        private void on_tock_event(object sender, TimerEventArgs e)
        {
            if (e.Ready)
            {
                tState = nextState;
            }
            else
            {
                tState = TState.wait;
            }
            //this should be set NLT T4 on each read/write cycle
            switch (S02)
            {
                case BusState.interruptAcknowledge:
                    inta();
                    break;
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
                        nextState = TState.none;
                        S02 = BusState.instructionFetch;
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void on_general_register_change(object sender, GeneralRegisterChangeEventArgs e)
        {
            GeneralRegisterChangeEvent?.Invoke(sender, e);
        }

        private void on_flag_change(object sender, FlagChangeEventArgs e)
        {
            FlagRegisterChangeEvent?.Invoke(sender, e);
        }

        private void on_segment_change(object sender, SegmentChangeEventArgs e)
        {
            SegmentChangeEvent?.Invoke(sender, e);
        }
        #endregion EVENTHANDLERS

        #region WORKERS
        private void inta()
        {
            switch (tState)
            {
                case TState.none:
                    InterruptController.Inta();
                    nextState = TState.data;
                    break;
                case TState.data:
                    temp = fbc.Data;
                    nextState = TState.clear;
                    break;
                case TState.clear:
                    fbc.Data = 0;
                    nextState = TState.none;
                    S02 = BusState.passive;
                    break;
                default:
                    throw new InvalidOperationException();
            }
        }

        private void get_instruction()
        {
            if (tState == TState.none)
            {
                workingSegment = Segment.CS;
                workingOffset = IP;
            }
            else if (tState == TState.clear)
            {
                InstructionQueue.Enqueue(temp);
                IP++;

                if (InstructionQueue.Count == 6 && S02 == BusState.instructionFetch)
                {
                    S02 = BusState.passive;
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
                    nextState = TState.address;
                    break;
                case TState.address: //begin T2
                                     //A16-A19 become S3-S6
                                     //AD0-AD7 clear
                    fbc.S34 = workingSegment;
                    fbc.S5 = InterruptEnabled;
                    nextState = TState.status;
                    break;
                case TState.status: //begin T3
                    nextState = TState.data;
                    break;
                case TState.data: //begin T4
                    temp = fbc.Data;

                    //#if DEBUG
                    //                        tempLow = memory[(segments[workingSegment] << 4) + workingOffset];
                    //#endif
                    nextState = TState.clear;
                    break;
                case TState.wait:
                    // does the BIU need to do anything here? ensure that a pin is set or something?
                    break;
                case TState.clear:
                    //onBusCycleComplete.Invoke();
                    fbc.Data = 0;
                    nextState = TState.none;
                    break;
            }
        }

        private void write_byte()
        {
            switch (tState)
            {
                case TState.none://begin T1
                    //Kludge
                    fbc.Data = temp;

                    fbc.Address = (segments[workingSegment] << 4) + workingOffset;
                    nextState = TState.address;
                    break;
                case TState.address: //begin T2
                    fbc.S34 = workingSegment;
                    fbc.S5 = InterruptEnabled;
                    fbc.Data = temp;
                    nextState = TState.status;
                    break;
                case TState.status: //begin T3
                    nextState = TState.data;
                    break;
                case TState.data: //begin T4
                    nextState = TState.clear;
                    //#if DEBUG
                    //                        memory[(segments[workingSegment] << 4) + workingOffset] = tempLow;
                    //#endif
                    break;
                case TState.wait:
                    break;
                case TState.clear:
                    fbc.Data = 0;
                    nextState = TState.none;
                    break;
            }
        }
        #endregion WORKERS

        #region JUMPS
        public void JumpNear(ushort offset)
        {
            mainTimer.TockEvent -= on_tock_event;

            nextState = TState.none;

            S02 = BusState.instructionFetch;

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

            nextState = TState.none;

            S02 = BusState.instructionFetch;

            segments.CS = newCS;

            IP = newIP;

            InstructionQueue.Clear();

            mainTimer.TockEvent += on_tock_event;
        }

        public void JumpImmediate(ushort newIP)
        {
            mainTimer.TockEvent -= on_tock_event;

            nextState = TState.none;

            S02 = BusState.instructionFetch;

            IP = newIP;

            InstructionQueue.Clear();

            mainTimer.TockEvent += on_tock_event;
        }

        public void JumpToInterruptVector(ushort interrupt)
        {
            ushort ip = ReadWord(Segment.IO, (ushort)(interrupt * 4));

            ushort cs = ReadWord(Segment.IO, (ushort)((interrupt * 4) + 2));

            mainTimer.TockEvent -= on_tock_event;

            nextState = TState.none;

            S02 = BusState.instructionFetch;

            IP = ip;

            segments.CS = cs;

            InstructionQueue.Clear();

            mainTimer.TockEvent += on_tock_event;
        }

        public void JumpShort(byte offset)
        {
            mainTimer.TockEvent -= on_tock_event;

            nextState = TState.none;

            S02 = BusState.instructionFetch;

            if ((offset & 0x80) != 0)
            {
                offset ^= 0xff;
                offset++;
                IP -= offset;
            }
            else
            {
                IP += offset;
                IP--;
            }

            InstructionQueue.Clear();

            mainTimer.TockEvent += on_tock_event;
        }
        #endregion JUMPS

        public Processor(FrontSideBusController fbc)
        {   
            executionUnit = new ExecutionUnit(this);
            segments.SegmentChangeHandler += on_segment_change;
            executionUnit.GeneralRegisterChangeEvent += on_general_register_change;
            executionUnit.FlagRegisterChangeEvent += on_flag_change;
            this.fbc = fbc;
            workingSegment = Segment.CS;
            IP = 0;
            nextState = TState.none;
            S02 = BusState.instructionFetch;
        }

        public void Start()
        {
            mainTimer.TockEvent += on_tock_event;
            executionUnit.Run();
        }

        internal void Stop()
        {
            executionUnit.End();
            mainTimer.TockEvent -= on_tock_event;
            Thread.Sleep(10);
            mainTimer.IsRunning = false;
            nextState = TState.clear;
            tState = TState.clear;
        }

        internal Tuple<SegmentRegisters,ushort,GeneralRegisters,FlagRegister> GetRegisters()
        {
            return new Tuple<SegmentRegisters, ushort, GeneralRegisters, FlagRegister>(segments, IP, executionUnit.Registers, executionUnit.flags);
        }

        internal void Intr()
        {

        }
    }
}
