using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SystemBoard.i8255;
using SystemBoard.i8259;

namespace SystemBoard.Keyboard
{
    public class KeyboardController : IPeripheral
    {
        private readonly PeripheralInterface ppi;
        private readonly InterruptController interruptController;
        private byte scanCode;

        public KeyboardController(EventHandler<PcKeyEventArgs> pcKeyHandler, PeripheralInterface ppi, InterruptController interruptController)
        {
            pcKeyHandler += OnPcKeyEvent;
            this.ppi = ppi;
            this.interruptController = interruptController;
        }

        protected void OnPcKeyEvent(object sender, PcKeyEventArgs e)
        {
            scanCode = e.ScanCode;
            interruptController.IRQ(1);
        }

        public byte Get()
        {
            throw new NotImplementedException();
        }

        public void Set(byte data)
        {
            throw new NotImplementedException();
        }
    }
}
