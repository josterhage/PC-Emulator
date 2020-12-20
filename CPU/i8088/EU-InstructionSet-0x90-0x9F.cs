using System;

namespace CPU.i8088
{
    public partial class Processor
    {
        private partial class ExecutionUnit
        {
            private void nop()
            {
                TempA = registers.AX;
                registers.AX = registers.AX;
                registers.AX = TempA;
            }

            private void xchg_ax_cx()
            {

                TempA = registers.AX;
                registers.AX = registers.CX;
                registers.CX = TempA;
            }

            private void xchg_ax_dx()
            {

                TempA = registers.AX;
                registers.AX = registers.DX;
                registers.DX = TempA;
            }

            private void xchg_ax_bx()
            {

                TempA = registers.AX;
                registers.AX = registers.BX;
                registers.BX = TempA;
            }

            private void xchg_ax_sp()
            {

                TempA = registers.AX;
                registers.AX = registers.SP;
                registers.SP = TempA;
            }

            private void xchg_ax_bp()
            {
                TempA = registers.AX;
                registers.AX = registers.BP;
                registers.BP = TempA;
            }

            private void xchg_ax_si()
            {

                TempA = registers.AX;
                registers.AX = registers.SI;
                registers.SI = TempA;
            }

            private void xchg_ax_di()
            {

                TempA = registers.AX;
                registers.AX = registers.DI;
                registers.DI = TempA;
            }

            private void cbw()
            {
                registers.AH = (byte)((registers.AL & 0x80) != 0 ? 0xff : 0);
            }

            private void cwd()
            {
                registers.DX = (ushort)((registers.AX & 0x8000) != 0 ? 0xffff : 0);
            }

            private void call_far()
            {
                //push cs
                push_cs();
                //push IP
                busInterfaceUnit.WriteIPToStack(registers.SP);
                registers.SP -= 2;

                //get the offset
                fetch_next_from_queue();
                tempAL = tempBL;
                fetch_next_from_queue();
                tempAH = tempBL;

                //get the segment
                fetch_next_from_queue();
                tempCL = tempBL;
                fetch_next_from_queue();
                tempCH = tempBL;

                //jump -> RET will assume that the next two values on the stack are the origin IP and CS, pop them, then jmp back
                busInterfaceUnit.JumpFar(TempC, TempA);
            }

            private void wait()
            {
                busInterfaceUnit.Wait();
            }

            private void pushf()
            {
                registers.SP -= 2;
                busInterfaceUnit.SetWord(BusInterfaceUnit.Segment.SS, registers.SP, flags.Flags);
            }

            private void popf()
            {
                flags.Flags = busInterfaceUnit.GetWord(BusInterfaceUnit.Segment.SS, registers.SP);
                registers.SP += 2;
            }

            private void sahf()
            {
                registers.AH = (byte)(flags.Flags & 0xff);
            }

            private void lahf()
            {
                TempA = flags.Flags;
                tempAL = registers.AH;
                flags.Flags = TempA;
            }
        }
    }
}

