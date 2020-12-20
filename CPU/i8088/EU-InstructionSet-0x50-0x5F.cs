using System;

namespace CPU.i8088
{
    public partial class Processor
    {
        private partial class ExecutionUnit
        {
            private void push_ax()
            {
                registers.SP -= 2;
                busInterfaceUnit.SetWord(BusInterfaceUnit.Segment.SS, registers.SP, registers.AX);
            }

            private void push_cx()
            {
                registers.SP -= 2;
                busInterfaceUnit.SetWord(BusInterfaceUnit.Segment.SS, registers.SP, registers.CX);
            }

            private void push_dx()
            {
                registers.SP -= 2;
                busInterfaceUnit.SetWord(BusInterfaceUnit.Segment.SS, registers.SP, registers.DX);
            }

            private void push_bx()
            {
                registers.SP -= 2;
                busInterfaceUnit.SetWord(BusInterfaceUnit.Segment.SS, registers.SP, registers.BX);
            }

            private void push_sp()
            {
                registers.SP -= 2;
                busInterfaceUnit.SetWord(BusInterfaceUnit.Segment.SS, registers.SP, registers.SP);
            }

            private void push_bp()
            {
                registers.SP -= 2;
                busInterfaceUnit.SetWord(BusInterfaceUnit.Segment.SS, registers.SP, registers.BP);
            }

            private void push_si()
            {
                registers.SP -= 2;
                busInterfaceUnit.SetWord(BusInterfaceUnit.Segment.SS, registers.SP, registers.SI);
            }

            private void push_di()
            {
                registers.SP -= 2;
                busInterfaceUnit.SetWord(BusInterfaceUnit.Segment.SS, registers.SP, registers.DI);
            }

            private void pop_ax()
            {
                registers.AX = busInterfaceUnit.GetWord(BusInterfaceUnit.Segment.SS, registers.SP);
                registers.SP += 2;
            }

            private void pop_cx()
            {
                registers.CX = busInterfaceUnit.GetWord(BusInterfaceUnit.Segment.SS, registers.SP);
                registers.SP += 2;
            }

            private void pop_dx()
            {
                registers.DX = busInterfaceUnit.GetWord(BusInterfaceUnit.Segment.SS, registers.SP);
                registers.SP += 2;
            }

            private void pop_bx()
            {
                registers.BX = busInterfaceUnit.GetWord(BusInterfaceUnit.Segment.SS, registers.SP);
                registers.SP += 2;
            }

            private void pop_sp()
            {
                registers.SP = busInterfaceUnit.GetWord(BusInterfaceUnit.Segment.SS, registers.SP);
                //testing appears to show that the value popped into SP is not changed
                //registers.SP += 2; 
            }

            private void pop_bp()
            {
                registers.BP = busInterfaceUnit.GetWord(BusInterfaceUnit.Segment.SS, registers.SP);
                registers.SP += 2;
            }

            private void pop_si()
            {
                registers.SI = busInterfaceUnit.GetWord(BusInterfaceUnit.Segment.SS, registers.SP);
                registers.SP += 2;
            }

            private void pop_di()
            {
                registers.DI = busInterfaceUnit.GetWord(BusInterfaceUnit.Segment.SS, registers.SP);
                registers.SP += 2;
            }
        }
    }
}

