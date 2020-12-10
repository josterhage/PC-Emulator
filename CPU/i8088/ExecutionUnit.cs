using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CPU.SystemClock;

namespace CPU.i8088
{
    public partial class Processor
    {
        private partial class ExecutionUnit
        {
            //TODO: does the execution unit need timer access? its operation is asynchronous by nature: it gets data from the queue one byte at a time, works on it, and blocks when no new data is available
            private readonly MainTimer mainTimer = MainTimer.GetInstance();
            //necessary
            private readonly BusInterfaceUnit busInterfaceUnit;


            private readonly GeneralRegisters registers = new GeneralRegisters();
            private readonly FlagRegister flags = new FlagRegister();

            private readonly Thread executionUnitThread;

            private bool isRunning = true;

            public ExecutionUnit(BusInterfaceUnit busInterfaceUnit)
            {
                this.busInterfaceUnit = busInterfaceUnit;
                InitializeInstructionSet();
                executionUnitThread = new Thread(new ThreadStart(run));
            }

            public void Run()
            {
                executionUnitThread.Start();
            }

            public void End()
            {
                isRunning = false;
            }

            /// <summary>
            /// Fetches the next byte from the queue then executes it
            /// </summary>
            private void run()
            {
                while (isRunning)
                {
                    tempBL = busInterfaceUnit.GetNextFromQueue();
                    instructions[tempBL]?.Invoke();
                    zeroizeTemps();
                }
            }

        }
    }
}