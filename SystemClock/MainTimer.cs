using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SystemClock
{
    /// <summary>
    /// Class <c>MainTimer</c> provides a timer which uses System.Diagnostics.Stopwatch to provide tocks
    /// <remarks>This is an interim solution until a permanent hardware based solution can be implemented providing a </remarks>
    /// </summary>
    public sealed class MainTimer
    {
        private static MainTimer instance = null;

        private readonly long ticks; // the number of ticks produced by System.Diagnostics.Stopwatch in 1/10e6 seconds

        private int tocks = 0;

        private readonly Thread timerThread;

        private readonly Stopwatch stopwatch;

        public bool IsRunning { get; set; } = true;

        public event EventHandler TockEvent; // I'm trying to ... mostly comply with .NET guidelines.
        public event EventHandler TwoTockEvent;
        public event EventHandler FourTockEvent;

        private MainTimer()
        {
#if DEBUG
            ticks = Stopwatch.Frequency / 2; // 2hz processor ftw
#else

            if (Stopwatch.IsHighResolution)
            {
                ticks = Stopwatch.Frequency / 1000000;
            }
            else
            {
                ticks = 1000000; // magic numbers, yay
            }
#endif
            stopwatch = new Stopwatch();
            timerThread = new Thread(new ThreadStart(counter_thread));
            timerThread.Start();
        }

        private void counter_thread()
        {
            if (!stopwatch.IsRunning)
            {
                stopwatch.Start();
            }
            while (IsRunning)
            {
                if (stopwatch.ElapsedTicks >= ticks)
                {
                    OnTock(new EventArgs());
                    stopwatch.Restart();
                }
            }
        }

        public void OnTock(EventArgs e)
        {
            EventHandler raiseEvent = TockEvent;

            raiseEvent?.Invoke(this, e);

            tocks++;
            if (tocks == 2)
            {
                raiseEvent = TwoTockEvent;
                raiseEvent?.Invoke(this, e);
            }
            else if (tocks == 4)
            {
                raiseEvent = TwoTockEvent;
                raiseEvent?.Invoke(this, e);
                raiseEvent = FourTockEvent;
                raiseEvent?.Invoke(this, e);
                tocks = 0;
            }

        }

        public static MainTimer GetInstance()
        {
            if (instance == null)
            {
                instance = new MainTimer();
            }

            return instance;
        }
    }
}
