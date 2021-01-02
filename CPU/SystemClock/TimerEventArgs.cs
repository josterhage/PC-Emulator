using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SystemBoard.SystemClock
{
    public class TimerEventArgs : EventArgs
    {
        public bool Ready;
        public bool Reset;
        public ulong Ticks;

        public TimerEventArgs(bool ready, bool reset, ulong ticks)
        {
            Ready = ready;
            Reset = reset;
            Ticks = ticks;
        }
    }
}
