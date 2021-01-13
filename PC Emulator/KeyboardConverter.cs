using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using SystemBoard;
using SystemBoard.Keyboard;

namespace PC_Emulator
{
    public class KeyboardConverter : IKeyboardConverter
    {
        private readonly static byte[] scanCodes = new byte[173]
        {
            0,0,14,15,0,0,28,0,58,0,0,0,0,1,0,0,0,0,57,73,81,79,71,75,72,77,80,0,0,0,55,82,83,0,11,2,3,4,5,6,7,8,9,10,30,48,46,32,18,
            33,34,35,23,36,37,38,50,49,24,25,16,19,31,20,22,47,17,45,21,44,0,0,0,0,82,79,80,81,75,76,77,71,72,73,0,78,0,74,0,0,59,60,
            61,62,63,64,65,66,67,68,0,0,0,0,0,0,0,0,0,0,0,0,0,0,69,70,42,54,29,0,56,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,39,13,51,12,52,
            53,41,0,0,26,43,27,40,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0
        };

        public event EventHandler<PcKeyEventArgs> PcKeyEvent;

        public void OnWindowsKeyEvent(object sender, KeyEventArgs e)
        {
            if (scanCodes[(int)e.Key] == 0)
                return;

            var scancode = scanCodes[(int)e.Key];

            scancode += e.IsUp ? 0x80 : 0;

            PcKeyEvent?.Invoke(sender, new PcKeyEventArgs(scancode));
        }
    }
}
