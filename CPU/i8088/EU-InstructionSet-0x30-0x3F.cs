using System;

namespace CPU.i8088
{
    public partial class Processor
    {
        private partial class ExecutionUnit
        {
            private void xor_rm8_r8()
            {
                fetch_next_from_queue();
                //If bits 6/7 are high then the destination is a register
                if ((ModEncoding)((tempBL & 0xC0) >> 6) == ModEncoding.registerRegister)
                {
                    // in register-> register operations bits 3-5 of the ModRM byte encode the src
                    var srcReg = (ByteGeneral)((tempBL & 0x38) >> 3);
                    // and bits 0-2 encode the dest
                    var destReg = (ByteGeneral)(tempBL & 0x07);

                    registers[destReg] = set_flags_and_xor(registers[destReg], registers[srcReg]);
                }
                else //the destination is a memory address
                {
                    //load the address
                    var srcReg = (ByteGeneral)((tempBL & 0x38) >> 3);
                    build_effective_address();

                    byte dest = busInterfaceUnit.GetByte(overrideSegment, TempC);

                    busInterfaceUnit.SetByte(overrideSegment, TempC, set_flags_and_xor(dest, registers[srcReg]));
                }
            }

            private void xor_rm16_r16()
            {
                fetch_next_from_queue();
                if ((ModEncoding)((tempBL & 0xC0) >> 6) == ModEncoding.registerRegister)
                {
                    var srcReg = (WordGeneral)((tempBL & 0x38) >> 3);
                    var destReg = (WordGeneral)(tempBL & 0x07);

                    registers[destReg] = set_flags_and_xor(registers[destReg], registers[srcReg]);
                }
                else
                {
                    var srcReg = (WordGeneral)((tempBL & 0x38) >> 3);
                    build_effective_address();

                    ushort dest = busInterfaceUnit.GetWord(overrideSegment, TempC);

                    TempA = set_flags_and_xor(registers[srcReg], dest);

                    busInterfaceUnit.SetWord(overrideSegment, TempC, set_flags_and_xor(dest, registers[srcReg]));
                }
            }

            private void xor_r8_rm8()
            {
                fetch_next_from_queue();
                //If bits 6/7 are high then the destination is a register
                if ((ModEncoding)((tempBL & 0xC0) >> 6) == ModEncoding.registerRegister)
                {
                    // in register-> register operations bits 3-5 of the ModRM byte encode the src
                    var destReg = (ByteGeneral)((tempBL & 0x38) >> 3);
                    // and bits 0-2 encode the dest
                    var srcReg = (ByteGeneral)(tempBL & 0x07);

                    registers[destReg] = set_flags_and_xor(registers[destReg], registers[srcReg]);
                }
                else //the destination is a memory address
                {
                    var destReg = (ByteGeneral)((tempBL & 0x38) >> 3);

                    //load the address
                    build_effective_address();
                    var src = busInterfaceUnit.GetByte(overrideSegment, TempC);

                    registers[destReg] = set_flags_and_xor(registers[destReg], src);
                }
            }

            private void xor_r16_rm16()
            {
                fetch_next_from_queue();
                //If bits 6/7 are high then the destination is a register
                if ((ModEncoding)((tempBL & 0xC0) >> 6) == ModEncoding.registerRegister)
                {
                    // in register-> register operations bits 3-5 of the ModRM byte encode the src
                    var destReg = (WordGeneral)((tempBL & 0x38) >> 3);
                    // and bits 0-2 encode the dest
                    var srcReg = (WordGeneral)(tempBL & 0x07);

                    registers[destReg] = set_flags_and_xor(registers[destReg], registers[srcReg]);
                }
                else //the destination is a memory address
                {
                    var destReg = (WordGeneral)((tempBL & 0x38) >> 3);

                    //load the address
                    build_effective_address();
                    ushort src = (ushort)(busInterfaceUnit.GetByte(overrideSegment, TempC) << 8);
                    src |= busInterfaceUnit.GetByte(overrideSegment, (ushort)(TempC + 1));

                    registers[destReg] = set_flags_and_xor(registers[destReg], src);
                }
            }

            private void xor_al_i8()
            {
                fetch_next_from_queue();
                registers.AL = set_flags_and_xor(registers.AL, tempBL);
            }

            private void xor_ax_i16()
            {
                fetch_next_from_queue();
                registers.AX = set_flags_and_xor(registers.AX, TempB);
            }

            private void override_ss()
            {
                overrideSegment = BusInterfaceUnit.Segment.SS;
                fetch_next_from_queue();
                instructions[tempBL]?.Invoke();
            }

            private void aaa()
            {
                if (((registers.AL & 0x0f) > 9) || flags.AF)
                {
                    registers.AX += 0x106;
                    flags.AF = true;
                    flags.CF = true;
                }
                else
                {
                    flags.AF = false;
                    flags.CF = false;
                }
                registers.AL &= 0x0f;
            }

            private void cmp_rm8_r8()
            {
                fetch_next_from_queue();
                //If bits 6/7 are high then the destination is a register
                if ((ModEncoding)((tempBL & 0xC0) >> 6) == ModEncoding.registerRegister)
                {
                    // in register-> register operations bits 3-5 of the ModRM byte encode the src
                    var srcReg = (ByteGeneral)((tempBL & 0x38) >> 3);
                    // and bits 0-2 encode the dest
                    var destReg = (ByteGeneral)(tempBL & 0x07);

                    tempAL = set_flags_and_diff(registers[destReg], registers[srcReg]);
                }
                else //the destination is a memory address
                {
                    //load the address
                    var srcReg = (ByteGeneral)((tempBL & 0x38) >> 3);
                    build_effective_address();

                    byte dest = busInterfaceUnit.GetByte(overrideSegment, TempC);

                    tempAL = set_flags_and_diff(dest, registers[srcReg]);
                }
            }

            private void cmp_rm16_r16()
            {
                fetch_next_from_queue();
                if ((ModEncoding)((tempBL & 0xC0) >> 6) == ModEncoding.registerRegister)
                {
                    var srcReg = (WordGeneral)((tempBL & 0x38) >> 3);
                    var destReg = (WordGeneral)(tempBL & 0x07);

                    TempA = set_flags_and_diff(registers[destReg], registers[srcReg]);
                }
                else
                {
                    var srcReg = (WordGeneral)((tempBL & 0x38) >> 3);

                    build_effective_address();

                    ushort dest = busInterfaceUnit.GetWord(overrideSegment, TempC);

                    TempA = set_flags_and_diff(dest, registers[srcReg]);
                }
            }

            private void cmp_r8_rm8()
            {
                fetch_next_from_queue();
                //If bits 6/7 are high then the destination is a register
                if ((ModEncoding)((tempBL & 0xC0) >> 6) == ModEncoding.registerRegister)
                {
                    // in register-> register operations bits 3-5 of the ModRM byte encode the src
                    var destReg = (ByteGeneral)((tempBL & 0x38) >> 3);
                    // and bits 0-2 encode the dest
                    var srcReg = (ByteGeneral)(tempBL & 0x07);

                    tempAL = set_flags_and_diff(registers[destReg], registers[srcReg]);
                }
                else //the destination is a memory address
                {
                    var destReg = (ByteGeneral)((tempBL & 0x38) >> 3);

                    //load the address
                    build_effective_address();
                    byte src = busInterfaceUnit.GetByte(overrideSegment, TempC);

                    tempAL = set_flags_and_diff(registers[destReg], src);
                }
            }

            private void cmp_r16_rm16()
            {
                fetch_next_from_queue();
                //If bits 6/7 are high then the destination is a register
                if ((ModEncoding)((tempBL & 0xC0) >> 6) == ModEncoding.registerRegister)
                {
                    // in register-> register operations bits 3-5 of the ModRM byte encode the src
                    var destReg = (WordGeneral)((tempBL & 0x38) >> 3);
                    // and bits 0-2 encode the dest
                    var srcReg = (WordGeneral)(tempBL & 0x07);

                    TempA = set_flags_and_diff(registers[destReg], registers[srcReg]);
                }
                else //the destination is a memory address
                {
                    var destReg = (WordGeneral)((tempBL & 0x38) >> 3);

                    //load the address
                    build_effective_address();

                    ushort src = busInterfaceUnit.GetWord(overrideSegment, TempC);

                    TempA = set_flags_and_diff(registers[destReg], src);
                }
            }

            private void cmp_al_i8()
            {
                fetch_next_from_queue();
                tempAL = set_flags_and_diff(registers.AL, tempBL);
            }

            private void cmp_ax_i16()
            {
                fetch_next_from_queue();
                TempA = set_flags_and_diff(registers.AX, TempB);
            }

            private void override_ds()
            {
                throw new NotImplementedException();
            }

            private void aas()
            {
                throw new NotImplementedException();
            }
        }
    }
}

