﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SystemBoard.Bus
{
    public enum BusState
    {
        interruptAcknowledge,
        readPort,
        writePort,
        halt,
        instructionFetch,
        readMemory,
        writeMemory,
        passive
    }
}
