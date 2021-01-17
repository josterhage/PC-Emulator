using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SystemBoard.i8255;
using SystemBoard.i8259;

namespace SystemBoard.Keyboard
{
    public class KeyboardController
    {
        private readonly InterruptController interruptController;
        private readonly Queue<byte> scanCodeBuffer;
        private const int MAX_BUFFER = 20;

        public bool PB6 { get; set; }

        public KeyboardController(InterruptController interruptController)
        {
            this.interruptController = interruptController;
            scanCodeBuffer = new Queue<byte>(MAX_BUFFER);
        }

        public void OnPcKeyEvent(object sender, PcKeyEventArgs e)
        {
            if (!PB6)
                return;
            if(scanCodeBuffer.Count < MAX_BUFFER)
            {
                scanCodeBuffer.Enqueue(e.ScanCode);
            }
            else
            {
                interruptController.IRQ(1);
            }
        }

        public byte Get()
        {
            if (scanCodeBuffer.Count > 0)
                return scanCodeBuffer.Dequeue();
            else
                return 0;
        }

        public void Set(byte data)
        {
            //silent failure
            return;
        }
    }
}
