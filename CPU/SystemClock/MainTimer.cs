using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


//TODO: Add methods/properties to handle wait state requests
namespace SystemBoard.SystemClock
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

        public ulong TotalTicks { get; private set; } = 0;

        public event EventHandler<TimerEventArgs> TockEvent; // I'm trying to ... mostly comply with .NET guidelines.
        public event EventHandler<TimerEventArgs> TwoTockEvent;
        public event EventHandler<TimerEventArgs> FourTockEvent;

        private MainTimer()
        {
#if DEBUG
            ticks = Stopwatch.Frequency / 10; // 2hz processor ftw
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
            while (true)
            {
                if (stopwatch.ElapsedTicks >= ticks)
                {
                    OnTock();
                    stopwatch.Restart();
                }
            }
        }

        public void OnTock()
        {
            EventHandler<TimerEventArgs> raiseEvent = TockEvent;

            raiseEvent?.Invoke(this, new TimerEventArgs(true,false,TotalTicks));

            TotalTicks++;

            tocks++;
            if (tocks == 2)
            {
                raiseEvent = TwoTockEvent;
                raiseEvent?.Invoke(this, new TimerEventArgs(true, false, TotalTicks));
            }
            else if (tocks == 4)
            {
                raiseEvent = TwoTockEvent;
                raiseEvent?.Invoke(this, new TimerEventArgs(true, false, TotalTicks));
                raiseEvent = FourTockEvent;
                raiseEvent?.Invoke(this, new TimerEventArgs(true, false, TotalTicks));
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
