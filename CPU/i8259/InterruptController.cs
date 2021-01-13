using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SystemBoard.Bus;
using SystemBoard.i8088;

namespace SystemBoard.i8259
{
    public class InterruptController : IMemoryLocation
    {
        /*
         *  This class is written to be a black box simulation of an i8529A chip
         *  It contains data structures for operation both in 8086/8088 and 8080/8085 modes
         *  As such, it could be used for an 8080/8085 emulator
         */

        private enum NextBytePortA
        {
            ICW, OCW, NONE
        }

        private enum NextBytePortB
        {
            ICW2, ICW3, ICW4, OCW1, NONE
        }

        private readonly FrontSideBusController bus;
        private readonly Processor cpu;
        private readonly InterruptController[] slaves;

        // Port expectation states
        private NextBytePortA portAStatus;
        private NextBytePortB portBStatus;

        private delegate void CommandWordHandler(byte data);

        // Port commands
        private Dictionary<NextBytePortA, CommandWordHandler> portACommands;
        private Dictionary<NextBytePortB, CommandWordHandler> portBCommands;

        private readonly bool sp; // pin 16, true for master, false for slave when ICW1b1 = 0;

        // Set by ICW 1-2
        private ushort vectorAddress; //Used for 8080/8085 mode

        // Set by ICW 2
        // In 8086/8088 mode the 8259a stores the most significant 5 bits of a one-byte
        // vector value, then adds 0-7 to get the final vector for each IRQ
        private byte vectorBase; //Used for 8086/8088 mode

        // The next two are set by ICW3, which is skipped when ICW1b1 = 1
        // when configured as a master in a cascade setup, the 8259a tracks which IRQs are connected
        // to slave 8259s - this is set in software with ICW3; ignored when ICW1b1 = 1
        private byte slaveList;

        // when configured as a slave in a cascade setup, the 8259a tracks which IRQ it's connected to for the master
        private byte slaveId;

        // configuration booleans
        private bool icw4b; // ICW1b0, tells whether the 8259a should wait for ICW4
        private bool single; // ICW1b1, true for single, false for cascade
        private bool adi; // ICW1b2, address interval - true for 4 byte, false for 8; irrelevant in 8086 mode
        //private bool ltim; // ltim is meaningless in software emulation
        private bool cpuType; // ICW4b0, false = 8080/8085 ;; true = 8086/8088
                              // Not emulating Automatic EOI for now
#if AEOIENABLED
        private bool aeoi; // ICW4b1, false = normal end of interrupt ;; true = automatic end of interrupt
#endif
        private bool masterSlave; // ICW4b2, master/slave, only meaning full if buf is true
        private bool buffered; // ICW4b3, buffered or not buffered
        private bool sfnm; // ICW4b4, special fully nested mode

        // priority rotation settings
        private bool sl; // OCW2b6
        private bool r; // OCW2b7

        private byte pivotIRQ;

        /*
         * RIS and RR work together for reading register values
         * RIS tells the 8259 which register (request or service) to read
         * RR tells the 8259 to read one or the other
         */
        private bool ris; // OCW3b0
        private bool rr; // OCW3b1
        private bool p; // OCW3b2
        private bool smm; // OCW3b5;
        private bool esmm; // OCW3b6;

        // bit masks
        private const byte ICW1MASK = 16;
        private const byte ICW1VECTORMASK = 224;
        private const byte ICW2TYPEMASK = 248;
        private const byte IRQLEVELMASK = 7;
        private const byte OCW3MASK = 8;

        private const byte EOIBIT = 32;
        private const byte SLBIT = 64;
        private const byte RBIT = 128;

        private byte interruptRequestRegister;
        private sbyte interruptServiceRegister = -1; // so that we can use -1 to track that no interrupt is being serviced
        private byte interruptMask;

        private byte intaCount;

        public int Size => 2;

        private readonly int _baseAddress;
        public int BaseAddress => _baseAddress;

        public void Write(int location, byte value)
        {
            //Port A
            if (location == _baseAddress)
            {
                portACommands[portAStatus].Invoke(value);
            }
            //Port B
            else if (location == _baseAddress + 1)
            {
                portBCommands[portBStatus].Invoke(value);
            }
            //Error
            else
            {
                throw new ArgumentOutOfRangeException();
            }
        }

        public byte Read(int location)
        {
            if (location == _baseAddress)
            {
                //TODO: handle the poll command?

                if (!rr)
                    return 0;

                if (ris)
                {
                    if (interruptServiceRegister < 0)
                    {
                        return 0;
                    }
                    else
                    {
                        return (byte)(1 << interruptServiceRegister);
                    }
                }
                else
                    return interruptRequestRegister;
            }
            else if (location == _baseAddress + 1)
            {
                return interruptMask;
            }
            else
            {
                throw new ArgumentOutOfRangeException();
            }
        }

        public InterruptController(Processor cpu, FrontSideBusController bus, int baseAddress)
        {
            this.cpu = cpu;
            this.bus = bus;
            _baseAddress = baseAddress;
            portAStatus = NextBytePortA.ICW;
            portBStatus = NextBytePortB.NONE;

            portACommands = new Dictionary<NextBytePortA, CommandWordHandler>()
            {
                {NextBytePortA.ICW,icw1 },
                {NextBytePortA.OCW,ocw2or3 },
                {NextBytePortA.NONE,portANone }
            };

            portBCommands = new Dictionary<NextBytePortB, CommandWordHandler>()
            {
                {NextBytePortB.ICW2,icw2 },
                {NextBytePortB.ICW3,icw3 },
                {NextBytePortB.ICW4,icw4 },
                {NextBytePortB.OCW1,ocw1 },
                {NextBytePortB.NONE,portBNone }
            };

            vectorAddress = 0;
            vectorBase = 0;
        }

        public InterruptController(Processor cpu, FrontSideBusController bus, int baseAddress, bool sp, InterruptController[] slaves) : this(cpu, bus, baseAddress)
        {
            this.sp = sp;
            //only copy the list, the byte containing the demuxed slaves is set in software
            this.slaves = slaves;
        }

        private void icw1(byte data)
        {
            //Silently fail if bit four isn't set
            if ((data & ICW1MASK) == 0)
                return;

            icw4b = (data & 1) != 0;
            single = (data & 2) != 0;
            adi = (data & 4) != 0;

            //we don't know if this is an 8080 or 8086 system, so we store the 8080 vector bits
            vectorAddress |= (ushort)(data & ICW1VECTORMASK);
            portAStatus = NextBytePortA.NONE;
            portBStatus = NextBytePortB.ICW2;
        }

        private void icw2(byte data)
        {
            //we don't know if this is an 8080 or 8086 system yet, so we store both sets of vector bits
            vectorAddress |= (ushort)(data << 8);

            vectorBase |= (byte)(data & ICW2TYPEMASK);

            // if the chip is in cascade mode we need to get ICW3
            // if not we need to see if icw4 has been requested
            portBStatus = single ? icw4b ? NextBytePortB.ICW4 : NextBytePortB.NONE : NextBytePortB.ICW3;
        }

        private void icw3(byte data)
        {
            if (sp)
            {
                slaveList = data;
            }
            else
            {
                slaveId = (byte)(data & IRQLEVELMASK);
            }
            portBStatus = icw4b ? NextBytePortB.ICW4 : NextBytePortB.NONE;
        }

        private void icw4(byte data)
        {
            cpuType = (data & 1) != 0;
#if AEOIENABLED
            aeoi = (data & 2) != 0;
#endif
            masterSlave = (data & 4) != 0;
            buffered = (data & 8) != 0;
            sfnm = (data & 16) != 0;
            portBStatus = NextBytePortB.OCW1;
        }

        private void ocw1(byte data)
        {
            interruptMask = data;
            portBStatus = NextBytePortB.NONE;
            portAStatus = NextBytePortA.OCW;
        }

        private void ocw2or3(byte data)
        {
            //OCW2
            if ((data & OCW3MASK) == 0)
            {
                // silently fail if 6 is the only instruction bit set
                if ((data & 224) == 64)
                    return;

                if ((data & SLBIT) != 0)
                {
                    //specific operation

                    //rotation and end of interrupt
                    if ((data & (RBIT | EOIBIT)) != 0)
                    {
                        //priority rotation
                        pivotIRQ = (byte)(1 << (data & 7));

                        //end of interrupt
                        eoi();

                        return;
                    }

                    if ((data & EOIBIT) != 0)
                    {
                        //end of specific interrupt
                        eoi((byte)(data & 7));

                        return;
                    }

                    if ((data & RBIT) != 0)
                    {
                        pivotIRQ = (byte)(1 << (data & 7));
                        return;
                    }

                    return;
                }
                else
                {
                    //non-specific operation
                    if ((data & (RBIT | EOIBIT)) != 0)
                    {
                        if (interruptServiceRegister < 0)
                            throw new InvalidOperationException();

                        pivotIRQ = (byte)interruptServiceRegister;

                        eoi();
                        return;
                    }

                    if ((data & EOIBIT) != 0)
                    {
                        eoi();
                        return;
                    }

                    //probably not necessary since i'm not simulating AEOI
                    r = (data & RBIT) != 0;
                }

                //r = (data & RBIT) != 0;
                //sl = ((data & SLBIT) != 0) && r;

                //if ((data & EOIBIT) != 0)
                //{
                //    if ((data & SLBIT) != 0)
                //        eoi(data);
                //    else
                //        eoi();
                //}
            }
            //OCW3
            else
            {
                ris = (data & 1) != 0;
                rr = (data & 2) != 0;
                p = (data & 4) != 0;
                if((data & 64) != 0)
                {
                    smm = (data & 32) != 0;
                }
            }
        }

        //There are very few circumstances in which these could be called, but in them we need the response to be silent failure
        private void portANone(byte data)
        {
            return; // ???
        }

        private void portBNone(byte data)
        {
            return; // ???
        }

        /*
         * 
         * 
         * Interrupt request sequence to this point:
         *  device asserts IRQ on line n (0-7)
         *  8259a checks if a mask has been declared for this IRQ, in which case the request is silently ignored
         *  otherwise the IRQ is asserted on the ISR and the an INTR signal is sent to the CPU
         */
        public void IRQ(byte n)
        {
            // 0-7 are the only valid IRQ lines
            if (n > 7)
                throw new ArgumentOutOfRangeException();

            //check whether or not this interrupt is being masked
            //if it is silently ignore the IRQ
            if ((interruptMask & (1 << n)) != 0)
                return;

            interruptRequestRegister |= (byte)(1 << n);

            cpu.INTR = true;    // the 8086/8088 don't latch the INTR, we're going to simulate this by making INTR a boolean property
                                    // that is treated as being held high unless this class changes it

        }

        /* This is a signal from the CPU that the interrupt has been received and can be handled
         * The next step depends on the value of ICW4b0 - high for 8086, low for 8080
         * In 8080 mode the 8259 places the opcode for CALL onto the data bus
         * In both modes the 8259 puts the highest priority IRQ from the IRR onto the ISR
        */
        public void Inta()
        {
            switch (intaCount)
            {
                case 0:
                    // first ack, in 8080 mode we're going to put the CALL opcode on the data bus
                    // in either mode we're going to put the pending request with the highest priority into the ISR

                    //8080 mode
                    if (!cpuType)
                    {
                        bus.Data = 0b11001101;
                    }

                    // find the highest-priorty irq after accounting for rotations

                    int priority = highestpriority();

                    if (priority < 0)
                    {
                        interruptServiceRegister = 7;
                        return;
                    }

                    // if this is the master in a cascade system and the high-priority request originated with a slave, we need to have the slave prepare
                    if (!single && sp && (slaveList & (1 << priority)) != 0)
                    {
                        slaves[priority].Inta();
                    }

                    // priority equals the highest-priority IRQ id, OR that bit to the ISR register
                    // per the datasheet, if the irq lines deassert before the intr/inta cycle completes the 8259 asserts IRQ7

                    interruptServiceRegister = (sbyte)(priority < 0 ? 7 : priority);

                    // the ISR no longer needs to track the IRQ because it's been found
                    interruptRequestRegister &= (byte)~priority;

                    intaCount++; // increment at the end because of the cases in which an already incremented count may become invalid
                    break;
                case 1:
                    //8080 mode
                    if (!cpuType)
                    {
                        //in 8080 mode we respond to the second inta line by asserting the least significant byte of the interrupt handler address onto the data bus

                        //if this is the master in a cascaded system and the interrupt belongs to a slave we need to tell it to assert its data onto the bus.
                        if (!single && sp && (slaveList & (1 << interruptServiceRegister)) != 0)
                        {
                            slaves[interruptServiceRegister].Inta();
                        }
                        //if any of the above conditions is false
                        else
                        {
                            bus.Data = adi ? (byte)((vectorAddress & 0xe0) | (interruptServiceRegister << 2)) : (byte)((vectorAddress & 0xd0) | (interruptServiceRegister << 3));
                        }
                        intaCount++;
                    }
                    else
                    {
                        //in 8086 mode we respond by asserting the vector number
                        if (!single && sp && (slaveList & (1 << interruptServiceRegister)) != 0)
                        {
                            slaves[interruptServiceRegister].Inta();
                        }
                        else
                        {
                            bus.Data = (byte)((vectorBase & 0xf8) | (byte)interruptServiceRegister);
                        }

                        intaCount = 0;
                        //code to emulate automatic end of interrupt functionality would go here
                        //i'm not emulating it because proper emulation would likely require a more precise emulation of 808x clock cycles
#if AEOIENABLED
                        //if automatic end of interrupt is enabled we need to end the interrupt after the next clock cycle
#endif
                    }
                    break;
                case 2:
                    //this should only be executed in 8080 mode, but let's double check
                    if (cpuType)
                        throw new InvalidOperationException();

                    //in 8080 mode we respond to the third inta by asserting the most significant bye of the interrupt handler
                    if (!single && sp && (slaveList & (1 << interruptServiceRegister)) != 0)
                    {
                        slaves[interruptServiceRegister].Inta();
                    }
                    else
                    {
                        bus.Data = (byte)((vectorAddress & 0xff00) >> 8);
                    }
                    //code to emulate automatic end of interrupt functionality would go here
                    //i'm not emulating it because proper emulation would likely require a more precise emulation of 808x clock cycles
#if AEOIENABLED
                        //if automatic end of interrupt is enabled we need to end the interrupt after the next clock cycle
#endif

                    break;
                default:
                    throw new InvalidOperationException();
            }
        }

        private void eoi()
        {
            // this overload is only called when the cpu calls with OCW2 and SLBIT is off
            // in this circumstance the 8259 would end whatever IRQ is currently being serviced.
            if (interruptServiceRegister < 0)
                throw new InvalidOperationException();

            eoi((byte)(1 << interruptServiceRegister));
        }

        private void eoi(byte vector)
        {
            interruptServiceRegister = -1;
        }

        // helper methods

        private int highestpriority()
        {
            int result = -1;

            byte lowPriorityMask = (byte)((1 << (pivotIRQ + 1)) - 1);

            byte isrPriority = (byte)((interruptRequestRegister & lowPriorityMask) << (7 - pivotIRQ));

            isrPriority |= (byte)(interruptRequestRegister >> (pivotIRQ + 1));

            int pos = 0;

            while (result < 0 && pos < 8)
            {
                result = (byte)(isrPriority & (1 << pos)) != 0 ? pos : -1;
                pos++;
            }

            if (result >= 0)
            {
                if (pos < (7 - pivotIRQ))
                {
                    return pos + pivotIRQ + 1;
                }
                else
                {
                    return pos - pivotIRQ + 1;
                }
            }

            return -1;
        }
    }
}
