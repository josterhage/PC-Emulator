using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace CPU
{
    public static class Oscillator
    {
        public static event EventHandler Tick;

#if DEBUG
        //for debugging we're going to use a System.Timer at 1/2 second for the system clock

        private static Timer mainClock;
        
#else

#endif

        public static void InitClock()
        {
            mainClock = new Timer(500);
            mainClock.Elapsed += OnMainClockElapsed;
            mainClock.AutoReset = true;
            mainClock.Start();
        }

        private static void OnMainClockElapsed(object sender, EventArgs e)
        {
            Tick?.Invoke(sender, e);
        }

        public static void EndClock()
        {
            mainClock.Stop();
            mainClock.Dispose();
        }
    }
}
