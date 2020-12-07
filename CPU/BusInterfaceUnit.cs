using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPU
{
    public class BusInterfaceUnit
    {
        private byte[] memory = new byte[1048576];

        private readonly Queue<byte> instructionQueue;

        private readonly RegisterSet registers;

        private readonly EventHandler tick;

        public bool QueueReady { get; private set; }

        public BusInterfaceUnit(RegisterSet registers)
        {
            this.registers = registers;
            instructionQueue = new Queue<byte>(10);
#if !DEBUG
            Oscillator.Tick += OnTick;
#endif
        }

        private void OnTick(object sender, EventArgs e)
        {
            tick?.Invoke(sender, e);
            if(instructionQueue.Count < 10)
            {
                instructionQueue.Enqueue(memory[(registers.CS << 4) + registers.IP]);
                registers.IP++;
                QueueReady = true;
            }
        }

#region debug

#if DEBUG
        //This method is only so that we can set memory values for testing
        //without using processor instructions
        public void SetMemory(uint address, byte value)
        {
            if (address > 0xFFFFF)
                throw new ArgumentOutOfRangeException();


            memory[address] = value;
        }

        public void WatchTicks()
        {
            Oscillator.Tick += OnTick;
        }
#endif

#endregion


        //private

        private byte fetch_byte(Segment segment, ushort offset)
        {
            switch (segment)
            {
                case Segment.ES:
                    return memory[(registers.CS << 4) + offset];
                case Segment.CS:
                    return memory[(registers.CS << 4) + offset];
                case Segment.SS:
                    return memory[(registers.SS << 4) + offset];
                case Segment.DS:
                    return memory[(registers.DS << 4) + offset];
                default:
                    return 0;
            }
        }

        private ushort fetch_word(Segment segment, ushort offset)
        {
            switch (segment)
            {
                case Segment.ES:
                    return (ushort)((ushort)(memory[(registers.ES << 4) + offset] << 8) | (memory[(registers.ES << 4) + offset + 1]));
                case Segment.CS:
                    return (ushort)((ushort)(memory[(registers.CS << 4) + offset] << 8) | (memory[(registers.CS << 4) + offset + 1]));
                case Segment.SS:
                    return (ushort)((ushort)(memory[(registers.SS << 4) + offset] << 8) | (memory[(registers.SS << 4) + offset + 1]));
                case Segment.DS:
                    return (ushort)((ushort)(memory[(registers.DS << 4) + offset] << 8) | (memory[(registers.DS << 4) + offset + 1]));
                default:
                    return 0;
            }
        }

        private void set_byte(Segment segment, ushort offset, byte value)
        {
            switch (segment)
            {
                case Segment.ES:
                    memory[(registers.ES << 4) + offset] = value;
                    break;
                case Segment.CS:
                    memory[(registers.CS << 4) + offset] = value;
                    break;
                case Segment.SS:
                    memory[(registers.SS << 4) + offset] = value;
                    break;
                case Segment.DS:
                    memory[(registers.DS << 4) + offset] = value;
                    break;
                default:
                    break;
            }
        }

        private void set_word(Segment segment, ushort offset, ushort value)
        {
            switch (segment)
            {
                case Segment.ES:
                    memory[(registers.ES << 4) + offset] = (byte)((value & 0xff00) >> 8);
                    memory[(registers.ES << 4) + offset + 1] = (byte)(value & 0x00ff);
                    break;
                case Segment.CS:
                    memory[(registers.CS << 4) + offset] = (byte)((value & 0xff00) >> 8);
                    memory[(registers.CS << 4) + offset + 1] = (byte)(value & 0x00ff);
                    break;
                case Segment.SS:
                    memory[(registers.SS << 4) + offset] = (byte)((value & 0xff00) >> 8);
                    memory[(registers.SS << 4) + offset + 1] = (byte)(value & 0x00ff);
                    break;
                case Segment.DS:
                    memory[(registers.DS << 4) + offset] = (byte)((value & 0xff00) >> 8);
                    memory[(registers.DS << 4) + offset + 1] = (byte)(value & 0x00ff);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public ushort FetchDataWord(ushort offset)
        {
            return fetch_word(Segment.DS, offset);
        }

        public byte FetchDataByte(ushort offset)
        {
            return fetch_byte(Segment.DS, offset);
        }

        public ushort InWord(ushort port)
        {
            return (ushort)((memory[port] << 8) | memory[port + 1]);
        }

        public byte InByte(ushort port)
        {
            return memory[port];
        }

        public void SetDataWord(ushort offset, ushort value)
        {
            set_word(Segment.DS, offset, value);
        }

        public void SetDataByte(ushort offset, byte value)
        {
            set_byte(Segment.DS, offset, value);
        }

        public void OutWord(ushort port, ushort value)
        {
            memory[port] = (byte)((value & 0xff00) >> 8);
            memory[port + 1] = (byte)(value * 0x00ff);
        }

        public void OutByte(ushort port, byte value)
        {
            memory[port] = value;
        }

        public byte? GetNextInstructionByte()
        {
            if(instructionQueue.Count == 0)
            {
                return null;
            }
            else
            {
                if (instructionQueue.Count == 1)
                    QueueReady = false;
                return instructionQueue.Dequeue();
            }
        }

        public void Push(ushort value)
        {
            set_word(Segment.SS, registers[WordGeneral.SP], value);
        }

        public ushort Pop()
        {
            return fetch_word(Segment.SS, registers[WordGeneral.SP]);
        }
    }
}
