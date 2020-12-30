using System;

namespace CPU.i8088
{
    public partial class Processor
    {
        private partial class ExecutionUnit
        {
            private void loopne_nz_i8()
            {
                registers.CX--;
                if (registers.CX == 0)
                    return;

                fetch_next_from_queue();

                if (!flags.ZF)
                {
                    busInterfaceUnit.JumpShort(tempBL);
                }
            }

            private void loope_z_i8()
            {
                registers.CX--;
                if (registers.CX == 0)
                    return;

                fetch_next_from_queue();

                if (flags.ZF)
                {
                    busInterfaceUnit.JumpShort(tempBL);
                }
            }

            private void loop_i8()
            {
                registers.CX--;
                if (registers.CX == 0)
                    return;

                fetch_next_from_queue();

                busInterfaceUnit.JumpShort(tempBL);
            }

            private void jcxz_i8()
            {
                fetch_next_from_queue();

                if (registers.CX == 0)
                {
                    busInterfaceUnit.JumpShort(tempBL);
                }
            }

            private void inb_i8()
            {
                fetch_next_from_queue();
                registers.AL = busInterfaceUnit.InByte(tempBL);
            }

            private void inw_i8()
            {
                fetch_next_from_queue();
                registers.AX = busInterfaceUnit.InWord(tempBL);
            }

            private void outb_i8()
            {
                fetch_next_from_queue();
                busInterfaceUnit.OutByte(tempBL, registers.AL);
            }

            private void outw_i8()
            {
                fetch_next_from_queue();
                busInterfaceUnit.OutWord(tempBL, registers.AX);
            }

            private void call_near_i16()
            {
                busInterfaceUnit.WriteIPToStack(registers.SP);
                registers.SP -= 2;

                //get the offset
                fetch_next_from_queue();
                tempAL = tempBL;
                fetch_next_from_queue();
                tempAH = tempBL;

                busInterfaceUnit.JumpNear(TempA);
            }

            private void jmp_near_i16()
            {
                fetch_next_from_queue();
                tempAL = tempBL;
                fetch_next_from_queue();
                tempAH = tempBL;

                busInterfaceUnit.JumpNear(TempA);
            }

            private void jmp_far_i16_i16()
            {
                // offset
                fetch_next_from_queue();
                tempAL = tempBL;
                fetch_next_from_queue();
                tempAH = tempBL;

                //segment
                fetch_next_from_queue();
                tempCL = tempBL;
                fetch_next_from_queue();
                tempCH = tempBL;

                busInterfaceUnit.JumpFar(TempC, TempA);
            }

            private void jmp_short_i8()
            {
                fetch_next_from_queue();

                busInterfaceUnit.JumpShort(tempBL);
            }

            private void inb_dx()
            {
                registers.AL = busInterfaceUnit.InByte(registers.DX);
            }

            private void inw_dx()
            {
                registers.AX = busInterfaceUnit.InWord(registers.DX);
            }

            private void outb_dx()
            {
                busInterfaceUnit.OutByte(registers.DX, registers.AL);
            }

            private void outw_dx()
            {
                busInterfaceUnit.OutWord(registers.DX, registers.AX);
            }
        }
    }
}
