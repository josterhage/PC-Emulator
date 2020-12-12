﻿using System;
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
#if DEBUG
        public partial class BusInterfaceUnit
#else
        private partial class BusInterfaceUnit
#endif
        {
            private readonly MainTimer mainTimer = MainTimer.GetInstance();

            // Do I need a subscribable handler?
            //private event EventHandler TockEvent;

#if DEBUG
            private byte[] memory = new byte[0x100000];
#endif

            private readonly SegmentRegisters segments = new SegmentRegisters();
            private ushort IP = 0;

            private Segment workingSegment;
            private ushort workingOffset = 0;

            private byte tempLow = 0;
            private byte tempHigh = 0;

            private bool HandlingInterrupt = false;

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

            //private Action onBusCycleComplete;

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

            private TState tState = TState.none;

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

            private enum TState
            {
                none,
                address,
                status,
                data,
                clear,
                wait
            }

            public BusInterfaceUnit()
            {
                mainTimer.TockEvent += on_tock_event;
                workingSegment = Segment.CS;
                s02 = BusState.instructionFetch;
            }

            public byte GetNextFromQueue()
            {
                while (InstructionQueue.Count == 0) ;

                return InstructionQueue.Dequeue();
            }

            public byte GetByte(Segment segment, ushort offset)
            {
                while (tState != TState.clear || s02 == BusState.halt || HandlingInterrupt) ;

                mainTimer.TockEvent -= on_tock_event;

                tState = TState.none;

                zeroize();

                workingSegment = segment;

                workingOffset = offset;

                s02 = BusState.readMemory;

                mainTimer.TockEvent += on_tock_event;

                while (tempLow == 0) ;

                byte result = tempLow;

                while (tState != TState.clear) ;

                s02 = BusState.passive;

                return result;
            }

            public ushort GetWord(Segment segment, ushort offset)
            {
                while (tState != TState.clear || s02 == BusState.halt || HandlingInterrupt) ;

                mainTimer.TockEvent -= on_tock_event;

                tState = TState.none;

                zeroize();

                workingSegment = segment;

                workingOffset = offset;

                s02 = BusState.readMemory;

                mainTimer.TockEvent += on_tock_event;

                while (tempLow == 0) ;

                ushort result = tempLow;

                while (tempHigh == 0) ;

                result |= (ushort)(tempHigh << 8);

                while (tState != TState.clear) ;

                s02 = BusState.passive;

                return result;
            }

            public void SetByte(Segment segment, ushort offset, byte value)
            {
                while (tState != TState.clear || s02 == BusState.halt || HandlingInterrupt) ;

                mainTimer.TockEvent -= on_tock_event;

                tState = TState.none;

                zeroize(); 
                
                workingSegment = segment;

                workingOffset = offset;

                tempLow = value;

                s02 = BusState.writeMemory;

                mainTimer.TockEvent += on_tock_event;

                while (tState != TState.clear) ;

                s02 = BusState.passive;

                
            }

            public void SetWord(Segment segment, ushort offset, ushort value)
            {
                while (tState != TState.clear || s02 == BusState.halt || HandlingInterrupt) ;

                mainTimer.TockEvent -= on_tock_event;

                tState = TState.none;

                zeroize();

                workingSegment = segment;

                workingOffset = offset;

                tempLow = (byte)(value & 0xFF);

                s02 = BusState.writeMemory;

                while (tState != TState.clear) ;

                mainTimer.TockEvent -= on_tock_event;

                tempLow = (byte)((value & 0xFF00) >> 8);

                mainTimer.TockEvent += single_cycle_write_handler;

                s02 = BusState.passive;

                mainTimer.TockEvent += on_tock_event;
            }

            public void WriteSegmentToMemory(Segment segment, ushort offset, Segment segmentOverride = Segment.DS)
            {
                SetWord(segmentOverride, offset, segments[segment]);
            }

            public void SetSegment(Segment segment, ushort value)
            {
                segments[segment] = value;
            }

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
                        throw new NotImplementedException();
                    case BusState.writePort:
                        throw new NotImplementedException();
                    case BusState.halt:
                        throw new NotImplementedException();
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
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
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
                    case TState.none:
                        //TODO: put address on bus;
                        tState = TState.address;
                        break;
                    case TState.address:
                        //A16-A19 become S3-S6
                        //AD0-AD7 clear
                        tState = TState.status;
                        break;
                    case TState.status:
                        //no change?
                        tState = TState.data;
                        break;
                    case TState.data:
#if DEBUG
                        tempLow = memory[segments[workingSegment] + workingOffset];
#endif
                        tState = TState.clear;
                        break;
                    case TState.wait:
                        // does the BIU need to do anything here? ensure that a pin is set or something?
                        break;
                    case TState.clear:
                        //onBusCycleComplete.Invoke();
                        zeroize();
                        tState = TState.none;
                        break;
                }
            }

            private void write_byte()
            {
                switch (tState)
                {
                    case TState.none:
                        tState = TState.address;
                        break;
                    case TState.address:
                        tState = TState.status;
                        break;
                    case TState.status:
                        tState = TState.data;
                        break;
                    case TState.data:
                        tState = TState.clear;
#if DEBUG
                        memory[segments[workingSegment] + workingOffset] = tempLow;
#endif
                        break;
                    case TState.wait:
                        break;
                    case TState.clear:
                        zeroize();
                        tState = TState.none;
                        break;
                }
            }

            private void zeroize()
            {
                workingOffset = 0;
                workingSegment = Segment.none;
                Temp = 0;
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