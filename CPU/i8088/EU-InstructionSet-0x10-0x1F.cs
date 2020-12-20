using System;

namespace CPU.i8088
{
    public partial class Processor
    {
        private partial class ExecutionUnit
        {
            private void adc_rm8_r8()
            {
                fetch_next_from_queue();
                //If bits 6/7 are high then the destination is a register
                if ((ModEncoding)((tempBL & 0xC0) >> 6) == ModEncoding.registerRegister)
                {
                    // in register-> register operations bits 3-5 of the ModRM byte encode the src
                    var srcReg = (ByteGeneral)((tempBL & 0x38) >> 3);
                    // and bits 0-2 encode the dest
                    var destReg = (ByteGeneral)(tempBL & 0x07);

                    registers[destReg] = set_flags_and_sum_carry(registers[destReg], registers[srcReg]);
                }
                else //the destination is a memory address
                {
                    var srcReg = (ByteGeneral)((tempBL & 0x38) >> 3);

                    //load the address
                    build_effective_address();

                    byte dest = busInterfaceUnit.GetByte(overrideSegment, TempC);

                    busInterfaceUnit.SetByte(overrideSegment, TempC, set_flags_and_sum_carry(dest, registers[srcReg]));
                }
            }

            private void adc_rm16_r16()
            {
                fetch_next_from_queue();
                if ((ModEncoding)((tempBL & 0xC0) >> 6) == ModEncoding.registerRegister)
                {
                    var srcReg = (WordGeneral)((tempBL & 0x38) >> 3);
                    var destReg = (WordGeneral)(tempBL & 0x07);

                    registers[destReg] = set_flags_and_sum_carry(registers[destReg], registers[srcReg]);
                }
                else
                {
                    var srcReg = (WordGeneral)((tempBL & 0x38) >> 3);

                    build_effective_address();

                    ushort dest = busInterfaceUnit.GetWord(overrideSegment, TempC);

                    TempA = set_flags_and_sum_carry(dest, registers[srcReg]);

                    busInterfaceUnit.SetWord(overrideSegment, TempC, TempA);
                }
            }

            private void adc_r8_rm8()
            {
                fetch_next_from_queue();
                //If bits 6/7 are high then the destination is a register
                if ((ModEncoding)((tempBL & 0xC0) >> 6) == ModEncoding.registerRegister)
                {
                    // in register-> register operations bits 3-5 of the ModRM byte encode the src
                    var destReg = (ByteGeneral)((tempBL & 0x38) >> 3);
                    // and bits 0-2 encode the dest
                    var srcReg = (ByteGeneral)(tempBL & 0x07);

                    registers[destReg] = set_flags_and_sum_carry(registers[destReg], registers[srcReg]);
                }
                else //the destination is a memory address
                {
                    var destReg = (ByteGeneral)((tempBL & 0x38) >> 3);

                    //load the address
                    build_effective_address();
                    byte src = busInterfaceUnit.GetByte(overrideSegment, TempC);

                    registers[destReg] = set_flags_and_sum_carry(registers[destReg], src);
                }
            }

            private void adc_r16_rm16()
            {
                fetch_next_from_queue();
                //If bits 6/7 are high then the destination is a register
                if ((ModEncoding)((tempBL & 0xC0) >> 6) == ModEncoding.registerRegister)
                {
                    // in register-> register operations bits 3-5 of the ModRM byte encode the src
                    var destReg = (WordGeneral)((tempBL & 0x38) >> 3);
                    // and bits 0-2 encode the dest
                    var srcReg = (WordGeneral)(tempBL & 0x07);

                    registers[destReg] = set_flags_and_sum_carry(registers[destReg], registers[srcReg]);
                }
                else //the destination is a memory address
                {
                    var destReg = (WordGeneral)((tempBL & 0x38) >> 3);

                    //load the address
                    build_effective_address();

                    ushort src = busInterfaceUnit.GetWord(overrideSegment, TempC);

                    registers[destReg] = set_flags_and_sum_carry(registers[destReg], src);
                }
            }

            private void adc_al_i8()
            {
                fetch_next_from_queue();
                registers.AL = set_flags_and_sum_carry(registers.AL, tempBL);
            }

            private void adc_ax_i16()
            {
                fetch_next_from_queue();
                registers.AX = set_flags_and_sum_carry(registers.AX, TempB);
            }

            private void push_ss()
            {
                //decrement stack pointer by two (x86 stacks grow downward)
                registers.SP -= 2;
                //write value of ES to SS:SP
                busInterfaceUnit.WriteSegmentToMemory(BusInterfaceUnit.Segment.SS, registers.SP, BusInterfaceUnit.Segment.SS);
            }

            private void pop_ss()
            {
                //read the value at SS:SP
                TempA = busInterfaceUnit.GetWord(BusInterfaceUnit.Segment.SS, registers.SP);
                //tell the BIU to set ES
                busInterfaceUnit.SetSegment(BusInterfaceUnit.Segment.SS, TempA);
                //increment the stack pointer by two
                registers.SP += 2;
            }

            private void sbb_rm8_r8()
            {
                fetch_next_from_queue();
                //If bits 6/7 are high then the destination is a register
                if ((ModEncoding)((tempBL & 0xC0) >> 6) == ModEncoding.registerRegister)
                {
                    // in register-> register operations bits 3-5 of the ModRM byte encode the src
                    var srcReg = (ByteGeneral)((tempBL & 0x38) >> 3);
                    // and bits 0-2 encode the dest
                    var destReg = (ByteGeneral)(tempBL & 0x07);

                    registers[destReg] = set_flags_and_diff_borrow(registers[destReg], registers[srcReg]);
                }
                else //the destination is a memory address
                {
                    var srcReg = (ByteGeneral)((tempBL & 0x38) >> 3);

                    //load the address
                    build_effective_address();

                    byte dest = busInterfaceUnit.GetByte(overrideSegment, TempC);

                    busInterfaceUnit.SetByte(overrideSegment, TempC, set_flags_and_diff_borrow(dest, registers[srcReg]));
                }
            }

            private void sbb_rm16_r16()
            {
                fetch_next_from_queue();
                if ((ModEncoding)((tempBL & 0xC0) >> 6) == ModEncoding.registerRegister)
                {
                    var srcReg = (WordGeneral)((tempBL & 0x38) >> 3);
                    var destReg = (WordGeneral)(tempBL & 0x07);

                    registers[destReg] = set_flags_and_diff_borrow(registers[destReg], registers[srcReg]);
                }
                else
                {
                    var srcReg = (WordGeneral)((tempBL & 0x38) >> 3);

                    build_effective_address();

                    ushort dest = busInterfaceUnit.GetWord(overrideSegment, TempC);

                    TempA = set_flags_and_diff_borrow(dest, registers[srcReg]);

                    busInterfaceUnit.SetWord(overrideSegment, TempC, TempA);
                }
            }

            private void sbb_r8_rm8()
            {
                fetch_next_from_queue();
                //If bits 6/7 are high then the destination is a register
                if ((ModEncoding)((tempBL & 0xC0) >> 6) == ModEncoding.registerRegister)
                {
                    // in register-> register operations bits 3-5 of the ModRM byte encode the src
                    var destReg = (ByteGeneral)((tempBL & 0x38) >> 3);
                    // and bits 0-2 encode the dest
                    var srcReg = (ByteGeneral)(tempBL & 0x07);

                    registers[destReg] = set_flags_and_diff_borrow(registers[destReg], registers[srcReg]);
                }
                else //the destination is a memory address
                {
                    var destReg = (ByteGeneral)((tempBL & 0x38) >> 3);

                    //load the address
                    build_effective_address();
                    byte src = busInterfaceUnit.GetByte(overrideSegment, TempC);

                    registers[destReg] = set_flags_and_diff_borrow(registers[destReg], src);
                }
            }

            private void sbb_r16_rm16()
            {
                fetch_next_from_queue();
                //If bits 6/7 are high then the destination is a register
                if ((ModEncoding)((tempBL & 0xC0) >> 6) == ModEncoding.registerRegister)
                {
                    // in register-> register operations bits 3-5 of the ModRM byte encode the src
                    var destReg = (WordGeneral)((tempBL & 0x38) >> 3);
                    // and bits 0-2 encode the dest
                    var srcReg = (WordGeneral)(tempBL & 0x07);

                    registers[destReg] = set_flags_and_diff_borrow(registers[destReg], registers[srcReg]);
                }
                else //the destination is a memory address
                {
                    var destReg = (WordGeneral)((tempBL & 0x38) >> 3);

                    //load the address
                    build_effective_address();

                    ushort src = busInterfaceUnit.GetWord(overrideSegment, TempC);

                    registers[destReg] = set_flags_and_diff_borrow(registers[destReg], src);
                }
            }

            private void sbb_al_i8()
            {
                fetch_next_from_queue();
                registers.AL = set_flags_and_diff_borrow(registers.AL, tempBL);
            }

            private void sbb_ax_i16()
            {
                fetch_next_from_queue();
                registers.AX = set_flags_and_diff_borrow(registers.AX, TempB);
            }

            private void push_ds()
            {
                registers.SP -= 2;
                busInterfaceUnit.WriteSegmentToMemory(BusInterfaceUnit.Segment.DS, registers.SP, BusInterfaceUnit.Segment.SS);
            }

            private void pop_ds()
            {
                TempA = busInterfaceUnit.GetWord(BusInterfaceUnit.Segment.SS, registers.SP);
                busInterfaceUnit.SetSegment(BusInterfaceUnit.Segment.SS, TempA);
                registers.SP += 2;
            }
        }
    }
}

