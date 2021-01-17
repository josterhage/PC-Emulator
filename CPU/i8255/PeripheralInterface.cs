using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SystemBoard.Bus;
using SystemBoard.Keyboard;
using SystemBoard.i8259;

namespace SystemBoard.i8255
{
    public class PeripheralInterface : IMemoryLocation
    {
        //This is going to be a bit hack-y because I really just want to make it work.

        private readonly KeyboardController kbc;

        private byte switch1Settings;
        private byte switch2Settings;

        private byte pb;

        //TODO: How does this interface with the appropriate handlers?
        public bool cassetteData { get; set; }
        public bool timerChannel2 { get; set; }
        public bool ioChk { get; set; }
        public bool rwChck { get; set; }

        private event EventHandler<PcKeyEventArgs> PcKeyEvent;


        private const int _baseAddress = 0x60;

        public PeripheralInterface(KeyboardController keyboardController)
        {
            kbc = keyboardController;
        }

        public int Size => 4;

        public int BaseAddress => _baseAddress;

        public byte Read(int location)
        {
            if (location - _baseAddress == 3)
            {
                // configuration
                return 0x99;
            }
            
            switch(location - _baseAddress)
            {
                case 0:
                    if((pb & 128) != 0)
                    {
                        return switch1Settings;
                    }
                    else
                    {
                        return kbc.Get();
                    }
                case 1:
                    return 0;
                case 2:
                    byte result = (byte)(cassetteData ? 16 : 0);
                    result |= (byte)(timerChannel2 ? 32 : 0);
                    result |= (byte)(ioChk ? 64 : 0);
                    result |= (byte)(rwChck ? 128 : 0);
                    return (byte)(switch2Settings | result);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void Write(int location, byte value)
        {
            if(location - _baseAddress == 3)
            {
                //set configuration


                //kludge - i'm just going to fail if a program tries to set a different value
                if (value != 0x99)
                    throw new InvalidOperationException();
            }
            else if( location - _baseAddress == 1)
            {
                pb = value;
                //PB0
                //TODO: connect this to a property on the 8253 class/object

                //PB1
                //TODO: connect this to the sound class for disabling speaker emulation

                //PB2
                //TODO: Memory size?

                //PB3
                //TODO: Connect to cassette emulation

                //These may just silently ignore any change: I think we can assume that our "RAM" will never fail a parity check
                //PB4
                //TODO: enable/disable RAM parity check

                //PB5
                //TODO: enable/disable IO Ram parity check

                //PB6
                kbc.PB6 = (value & 64) != 0;

                //PB7
                
            }
        }

        public void SetSwitches(ushort value)
        {
            switch1Settings = (byte)(value & 0xff);
            switch2Settings = (byte)((value & 0xff00) >> 8);
        }

        public void OnPcKeyEvent(object sender, PcKeyEventArgs e)
        {
            kbc.OnPcKeyEvent(sender, e);
        }
    }
}
