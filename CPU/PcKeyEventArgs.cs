using System;

namespace SystemBoard
{
    public class PcKeyEventArgs : EventArgs
    {
        public byte ScanCode { get; set; }

        public PcKeyEventArgs(byte scanCode)
        {
            ScanCode = scanCode;
        }
    }
}