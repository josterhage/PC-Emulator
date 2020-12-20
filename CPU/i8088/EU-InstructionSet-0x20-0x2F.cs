using System;

namespace CPU.i8088
{
    public partial class Processor
    {
        private partial class ExecutionUnit
        {
            private void and_rm8_r8()
            {
                fetch_next_from_queue();
                //If bits 6/7 are high then the destination is a register
                if ((ModEncoding)((tempBL & 0xC0) >> 6) == ModEncoding.registerRegister)
                {
                    // in register-> register operations bits 3-5 of the ModRM byte encode the src
                    var srcReg = (ByteGeneral)((tempBL & 0x38) >> 3);
                    // and bits 0-2 encode the dest
                    var destReg = (ByteGeneral)(tempBL & 0x07);

                    registers[destReg] = set_flags_and_and(registers[destReg], registers[srcReg]);
                }
                else //the destination is a memory address
                {
                    //load the address
                    var srcReg = (ByteGeneral)((tempBL & 0x38) >> 3);
                    build_effective_address();

                    byte dest = busInterfaceUnit.GetByte(overrideSegment, TempC);

                    busInterfaceUnit.SetByte(overrideSegment, TempC, set_flags_and_and(dest, registers[srcReg]));
                }
            }

            private void and_rm16_r16()
            {
                fetch_next_from_queue();
                if ((ModEncoding)((tempBL & 0xC0) >> 6) == ModEncoding.registerRegister)
                {
                    var srcReg = (WordGeneral)((tempBL & 0x38) >> 3);
                    var destReg = (WordGeneral)(tempBL & 0x07);

                    registers[destReg] = set_flags_and_and(registers[destReg], registers[srcReg]);
                }
                else
                {
                    var srcReg = (WordGeneral)((tempBL & 0x38) >> 3);
                    build_effective_address();

                    ushort dest = busInterfaceUnit.GetWord(overrideSegment, TempC);

                    TempA = set_flags_and_and(registers[srcReg], dest);

                    busInterfaceUnit.SetWord(overrideSegment, TempC, set_flags_and_and(dest, registers[srcReg]));
                }
            }

            private void and_r8_rm8()
            {
                fetch_next_from_queue();
                //If bits 6/7 are high then the destination is a register
                if ((ModEncoding)((tempBL & 0xC0) >> 6) == ModEncoding.registerRegister)
                {
                    // in register-> register operations bits 3-5 of the ModRM byte encode the src
                    var destReg = (ByteGeneral)((tempBL & 0x38) >> 3);
                    // and bits 0-2 encode the dest
                    var srcReg = (ByteGeneral)(tempBL & 0x07);

                    registers[destReg] = set_flags_and_and(registers[destReg], registers[srcReg]);
                }
                else //the destination is a memory address
                {
                    var destReg = (ByteGeneral)((tempBL & 0x38) >> 3);

                    //load the address
                    build_effective_address();
                    var src = busInterfaceUnit.GetByte(overrideSegment, TempC);

                    registers[destReg] = set_flags_and_and(registers[destReg], src);
                }
            }

            private void and_r16_rm16()
            {
                fetch_next_from_queue();
                //If bits 6/7 are high then the destination is a register
                if ((ModEncoding)((tempBL & 0xC0) >> 6) == ModEncoding.registerRegister)
                {
                    // in register-> register operations bits 3-5 of the ModRM byte encode the src
                    var destReg = (WordGeneral)((tempBL & 0x38) >> 3);
                    // and bits 0-2 encode the dest
                    var srcReg = (WordGeneral)(tempBL & 0x07);

                    registers[destReg] = set_flags_and_and(registers[destReg], registers[srcReg]);
                }
                else //the destination is a memory address
                {
                    var destReg = (WordGeneral)((tempBL & 0x38) >> 3);

                    //load the address
                    build_effective_address();
                    ushort src = (ushort)(busInterfaceUnit.GetByte(overrideSegment, TempC) << 8);
                    src |= busInterfaceUnit.GetByte(overrideSegment, (ushort)(TempC + 1));

                    registers[destReg] = set_flags_and_and(registers[destReg], src);
                }
            }

            private void and_al_i8()
            {
                fetch_next_from_queue();
                registers.AL = set_flags_and_and(registers.AL, tempBL);
            }

            private void and_ax_i16()
            {
                fetch_next_from_queue();
                registers.AX = set_flags_and_and(registers.AX, TempB);
            }

            private void override_es()
            {
                overrideSegment = BusInterfaceUnit.Segment.ES;
                fetch_next_from_queue();
                instructions[tempBL]?.Invoke();
            }

            private void daa()
            {
                var carry = flags.CF;
                tempAL = registers.AL;
                flags.CF = false;

                if (((registers.AL & 0x0f) > 9) || flags.AF)
                {
                    registers.AL += 6;
                    flags.CF = carry;
                    flags.AF = true;
                }
                else
                {
                    flags.AF = false;
                }
                if ((tempAL > 0x99) || carry)
                {
                    registers.AL += 0x60;
                    flags.CF = true;
                }
                else
                {
                    flags.CF = false;
                }
            }

            private void sub_rm8_r8()
            {
                fetch_next_from_queue();
                //If bits 6/7 are high then the destination is a register
                if ((ModEncoding)((tempBL & 0xC0) >> 6) == ModEncoding.registerRegister)
                {
                    // in register-> register operations bits 3-5 of the ModRM byte encode the src
                    var srcReg = (ByteGeneral)((tempBL & 0x38) >> 3);
                    // and bits 0-2 encode the dest
                    var destReg = (ByteGeneral)(tempBL & 0x07);

                    registers[destReg] = set_flags_and_diff(registers[destReg], registers[srcReg]);
                }
                else //the destination is a memory address
                {
                    var srcReg = (ByteGeneral)((tempBL & 0x38) >> 3);

                    //load the address
                    build_effective_address();

                    byte dest = busInterfaceUnit.GetByte(overrideSegment, TempC);

                    busInterfaceUnit.SetByte(overrideSegment, TempC, set_flags_and_diff(dest, registers[srcReg]));
                }
            }

            private void sub_rm16_r16()
            {
                fetch_next_from_queue();
                if ((ModEncoding)((tempBL & 0xC0) >> 6) == ModEncoding.registerRegister)
                {
                    var srcReg = (WordGeneral)((tempBL & 0x38) >> 3);
                    var destReg = (WordGeneral)(tempBL & 0x07);

                    registers[destReg] = set_flags_and_diff(registers[destReg], registers[srcReg]);
                }
                else
                {
                    var srcReg = (WordGeneral)((tempBL & 0x38) >> 3);

                    build_effective_address();

                    ushort dest = busInterfaceUnit.GetWord(overrideSegment, TempC);

                    TempA = set_flags_and_diff(dest, registers[srcReg]);

                    busInterfaceUnit.SetWord(overrideSegment, TempC, TempA);
                }
            }

            private void sub_r8_rm8()
            {
                fetch_next_from_queue();
                //If bits 6/7 are high then the destination is a register
                if ((ModEncoding)((tempBL & 0xC0) >> 6) == ModEncoding.registerRegister)
                {
                    // in register-> register operations bits 3-5 of the ModRM byte encode the src
                    var destReg = (ByteGeneral)((tempBL & 0x38) >> 3);
                    // and bits 0-2 encode the dest
                    var srcReg = (ByteGeneral)(tempBL & 0x07);

                    registers[destReg] = set_flags_and_diff(registers[destReg], registers[srcReg]);
                }
                else //the destination is a memory address
                {
                    var destReg = (ByteGeneral)((tempBL & 0x38) >> 3);

                    //load the address
                    build_effective_address();
                    byte src = busInterfaceUnit.GetByte(overrideSegment, TempC);

                    registers[destReg] = set_flags_and_diff(registers[destReg], src);
                }
            }

            private void sub_r16_rm16()
            {
                fetch_next_from_queue();
                //If bits 6/7 are high then the destination is a register
                if ((ModEncoding)((tempBL & 0xC0) >> 6) == ModEncoding.registerRegister)
                {
                    // in register-> register operations bits 3-5 of the ModRM byte encode the src
                    var destReg = (WordGeneral)((tempBL & 0x38) >> 3);
                    // and bits 0-2 encode the dest
                    var srcReg = (WordGeneral)(tempBL & 0x07);

                    registers[destReg] = set_flags_and_diff(registers[destReg], registers[srcReg]);
                }
                else //the destination is a memory address
                {
                    var destReg = (WordGeneral)((tempBL & 0x38) >> 3);

                    //load the address
                    build_effective_address();

                    ushort src = busInterfaceUnit.GetWord(overrideSegment, TempC);

                    registers[destReg] = set_flags_and_diff(registers[destReg], src);
                }
            }

            private void sub_al_i8()
            {
                fetch_next_from_queue();
                registers.AL = set_flags_and_diff(registers.AL, tempBL);
            }

            private void sub_ax_i16()
            {
                fetch_next_from_queue();
                registers.AX = set_flags_and_diff(registers.AX, TempB);
            }

            private void override_cs()
            {
                overrideSegment = BusInterfaceUnit.Segment.CS;
                fetch_next_from_queue();
                instructions[tempBL]?.Invoke();
            }

            private void das()
            {
                var carry = flags.CF;
                tempAL = registers.AL;
                flags.CF = false;

                if (((registers.AL & 0x0f) > 9) || flags.AF)
                {
                    registers.AL -= 6;
                    flags.CF = carry;
                    flags.AF = true;
                }
                else
                {
                    flags.AF = false;
                }

                if ((tempAL > 0x99) || carry)
                {
                    registers.AL -= 0x60;
                    flags.CF = true;
                }
                else
                {
                    flags.CF = false;
                }
            }
        }
    }
}

