﻿using System;
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

        public bool EOP { get; set; }

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

        public DmaController()
        {
            timer.TockEvent += on_tock;
        }

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
            int xferMode = cycleState > 0 ? (modes[activeDreq] & transferMode) >> 6 : -1;
            bool increment = cycleState > 0 && (modes[activeDreq] & transferIncrementDecrement) != 0;
            bool autoinit = cycleState > 0 && (modes[activeDreq] & autoInit) != 0;
            int readwriteverify = cycleState > 0 ? (modes[activeDreq] & verWriteRead) >> 2 : -1;
            bool memtomem = cycleState > 0 && activeDreq == 0 && ((command & 1) != 0);
            byte tcMask = (byte)(cycleState > 0 && activeDreq >= 0 ? 1 << activeDreq : 0);
            byte reqMask = (byte)(cycleState > 0 && activeDreq >= 0 ? 1 << (activeDreq + 4) : 0);

            int a; // throwaway for polling drqs

            switch (cycleState)
            {
                case -1:
                    cycleState++;
                    return;
                case 0:
                    //poll the DRQ lines
                    activeDreq = poll_drq();
                    if (activeDreq < 0)
                        return;

                    status |= (byte)(1 << (activeDreq + 4));

                    if (masks[activeDreq])
                    {
                        activeDreq = -1;
                        return;
                    }

                    frontSideBusController.Hold = true;

                    while (!_hlda) ;

                    cycleState = memtomem ? 11 : 1;
                    return;
                //setup bus for transfer and send appropriate dack
                case 1:
                    //ensure the DREQ line is still active, clear the DREQ init and release the bus if it isn't
                    a = poll_drq();
                    if (a != activeDreq)
                    {
                        deactivate();
                        return;
                    }
                    // functional priority: single, block, demand, cascade
                    if (xferMode < 3)
                    {
                        //verify or read
                        if ((readwriteverify & 1) == 0)
                        {
                            frontSideBusController.S02 = BusState.readMemory;
                        }
                        //write
                        else if (readwriteverify == 1)
                        {
                            frontSideBusController.S02 = BusState.writeMemory;
                        }
                        else
                        {
                            //TODO: dive into datasheet, is this really what sould happen?

                            //this attempts to recover in the event bad "hardware" really borqs things up
                            status &= (byte)(0xff ^ (1 << (activeDreq + 4)));
                            activeDreq = -1;
                            cycleState = 0;
                            _hlda = false;
                            frontSideBusController.Hold = false;
                            return;
                        }

                        //put the address on the bus, the actual 8237 does this over two clock cycles, low-byte first
                        //since FrontSideBusController follows the clock cycling of an 8088 we don't have to cycle match here
                        frontSideBusController.Address = (pages[activeDreq] << 16) | currentRegisters[activeDreq * 2];
                        dack();
                        cycleState++;
                        return;
                    }
                    else if (xferMode == 3)
                    {
                        // in cascade mode we only need to let the slave 8237 know that it has control of the bus
                        dack();
                        cycleState++;
                        return;
                    }
                    else
                        throw new InvalidOperationException();
                case 2:
                    cycleState++;
                    return;
                case 3:
                    cycleState++;
                    return;
                case 4:
                    // decrement the word count register
                    currentRegisters[(activeDreq * 2) + 1]--;

                    // update the address register
                    if (increment)
                    {
                        currentRegisters[activeDreq * 2]--;
                    }
                    else
                    {
                        currentRegisters[activeDreq * 2]++;
                    }

                    //terminal count
                    if (currentRegisters[(activeDreq * 2) + 1] == 0xffff)
                    {
                        //set tc
                        status |= tcMask;
                    }

                    //I assume based on the data sheet that nothing happens after a terminal count if autoinit and EOP are false
                    if (((status & tcMask) != 0) && !EOP)
                    {
                        if (!autoinit)
                            return;

                        EOP = true;
                    } 

                    if (EOP)
                    {
                        currentRegisters[activeDreq * 2] = baseRegisters[activeDreq * 2];
                        currentRegisters[(activeDreq * 2) + 1] = baseRegisters[(activeDreq * 2) + 1];
                        deactivate();
                        return;
                    }

                    if(xferMode == 0)
                    {
                        a = poll_drq();
                        //in demand mode we return control to the bus if DREQ goes low before cycle four begins
                        if (a != activeDreq)
                        {
                            deactivate();
                            return;
                        }
                        cycleState = 1;
                        return;
                    }

                    if(xferMode == 1)
                    {
                        deactivate();
                        cycleState = -1; // wait one clock cycle before polling DREQ lines to ensure the processor can execute at least one instruction cycle
                        return;
                    }

                    if(xferMode == 2)
                    {
                        cycleState = 1;
                        return;
                    }

                    if(xferMode == 3)
                    {
                        cycleState = 0; /// ???
                    }

                    return;

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

        private void dack()
        {
            if (DREQ0)
                DACK0 = true;
            if (DREQ1)
                DACK1 = true;
            if (DREQ2)
                DACK2 = true;
            if (DREQ3)
                DACK3 = true;
        }

        private void deactivate()
        {
            status = 0;
            activeDreq = -1;
            cycleState = 0;
            _hlda = false;
            frontSideBusController.Hold = false;
        }
    }
}
