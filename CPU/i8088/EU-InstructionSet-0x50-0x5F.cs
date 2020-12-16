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
                //busInterfaceUnit.SetWord(BusInterfaceUnit.Segment.SS, registers.SP, registers.AX);
            }

            private void push_cx()
            {
                throw new NotImplementedException();
            }

            private void push_dx()
            {
                throw new NotImplementedException();
            }

            private void push_bx()
            {
                throw new NotImplementedException();
            }

            private void push_sp()
            {
                throw new NotImplementedException();
            }

            private void push_bp()
            {
                throw new NotImplementedException();
            }

            private void push_si()
            {
                throw new NotImplementedException();
            }

            private void push_di()
            {
                throw new NotImplementedException();
            }

            private void pop_ax()
            {
                throw new NotImplementedException();
            }

            private void pop_cx()
            {
                throw new NotImplementedException();
            }

            private void pop_dx()
            {
                throw new NotImplementedException();
            }

            private void pop_bx()
            {
                throw new NotImplementedException();
            }

            private void pop_sp()
            {
                throw new NotImplementedException();
            }

            private void pop_bp()
            {
                throw new NotImplementedException();
            }

            private void pop_si()
            {
                throw new NotImplementedException();
            }

            private void pop_di()
            {
                throw new NotImplementedException();
            }
        }
    }
}

