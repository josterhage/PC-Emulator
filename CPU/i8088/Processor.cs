using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPU.i8088
{
    public partial class Processor
    {
        private readonly ExecutionUnit executionUnit;
        private readonly BusInterfaceUnit biu;

        public Processor()
        {
            biu = new BusInterfaceUnit();
            executionUnit = new ExecutionUnit(biu);
            executionUnit.Run();
        }

#if DEBUG
        public Processor(BusInterfaceUnit busInterfaceUnit)
        {
            biu = busInterfaceUnit;
            executionUnit = new ExecutionUnit(biu);
            executionUnit.Run();
        }
#endif
    }
}
