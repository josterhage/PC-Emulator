using System;

namespace CPU.i8088
{
    public partial class Processor
    {
        private partial class ExecutionUnit
        {
            //0xc0 and 0xc1 have no instructions

            private void ret_near_i16()
            {
                TempA = busInterfaceUnit.GetWord(BusInterfaceUnit.Segment.SS, registers.SP);
                registers.SP += 2;
                fetch_next_from_queue();
                registers.SP += tempBL;
                busInterfaceUnit.JumpImmediate(TempA);
            }

            private void ret_near()
            {
                TempA = busInterfaceUnit.GetWord(BusInterfaceUnit.Segment.SS, registers.SP);
                registers.SP += 2;
                busInterfaceUnit.JumpImmediate(TempA);
            }

            private void lds_rm16()
            {
                fetch_next_from_queue();
                var destReg = (WordGeneral)(tempBL & 0x07);
                build_effective_address();
                TempA = busInterfaceUnit.GetWord(overrideSegment, TempC);
                TempB = busInterfaceUnit.GetWord(overrideSegment, (ushort)(TempC + 2));
                busInterfaceUnit.SetSegment(BusInterfaceUnit.Segment.DS, TempA);
                registers[destReg] = TempB;
            }

            private void les_rm16()
            {
                fetch_next_from_queue();
                var destReg = (WordGeneral)(tempBL & 0x07);
                build_effective_address();
                TempA = busInterfaceUnit.GetWord(overrideSegment, TempC);
                TempB = busInterfaceUnit.GetWord(overrideSegment, (ushort)(TempC + 2));
                busInterfaceUnit.SetSegment(BusInterfaceUnit.Segment.ES, TempA);
                registers[destReg] = TempB;
            }

            private void mov_m8_i8()
            {
                fetch_next_from_queue();
                build_effective_address();
                fetch_next_from_queue();
                busInterfaceUnit.SetByte(overrideSegment, TempC, tempBL);
            }

            private void mov_m16_i16()
            {
                fetch_next_from_queue();
                build_effective_address();
                fetch_next_from_queue();
                tempAL = tempBL;
                fetch_next_from_queue();
                tempAH = tempBL;
                busInterfaceUnit.SetWord(overrideSegment, TempC, TempA);
            }

            //0xc8 and 0xc9 have no instructions

            private void ret_far_i16()
            {
                TempA = busInterfaceUnit.GetWord(BusInterfaceUnit.Segment.SS, registers.SP);
                registers.SP += 2;
                TempB = busInterfaceUnit.GetWord(BusInterfaceUnit.Segment.SS, registers.SP);
                registers.SP += 2;
                fetch_next_from_queue();
                registers.SP += tempBL;
                busInterfaceUnit.JumpFar(TempB, TempA);
            }

            private void ret_far()
            {
                TempA = busInterfaceUnit.GetWord(BusInterfaceUnit.Segment.SS, registers.SP);
                registers.SP += 2;
                TempB = busInterfaceUnit.GetWord(BusInterfaceUnit.Segment.SS, registers.SP);
                registers.SP += 2;
                busInterfaceUnit.JumpFar(TempB, TempA);
            }

            private void int_3()
            {
                pushf();
                flags.IF = false;
                flags.TF = false;
                flags.AF = false;
                push_cs();
                busInterfaceUnit.WriteIPToStack(registers.SP);
                registers.SP -= 2;
                busInterfaceUnit.JumpToInterruptVector(3);
            }

            private void int_i8()
            {
                pushf();
                flags.IF = false;
                flags.TF = false;
                flags.AF = false;
                fetch_next_from_queue();
                push_cs();
                busInterfaceUnit.WriteIPToStack(registers.SP);
                registers.SP -= 2;
                busInterfaceUnit.JumpToInterruptVector(tempBL);
            }

            private void int_o()
            {
                if (flags.OF)
                {
                    pushf();
                    flags.IF = false;
                    flags.TF = false;
                    flags.AF = false;
                    push_cs();
                    busInterfaceUnit.WriteIPToStack(registers.SP);
                    registers.SP -= 2;
                    busInterfaceUnit.JumpToInterruptVector(4);
                }
            }

            private void iret()
            {
                TempA = busInterfaceUnit.GetWord(BusInterfaceUnit.Segment.SS, registers.SP);
                registers.SP += 2;
                TempB = busInterfaceUnit.GetWord(BusInterfaceUnit.Segment.SS, registers.SP);
                registers.SP += 2;
                popf();
                busInterfaceUnit.JumpFar(TempB, TempA);
            }
        }
    }
}
