using System;

namespace CPU.i8088
{
    public partial class Processor
    {
        private partial class ExecutionUnit
        {
            private void add_rm8_r8()
            {
                fetch_next_from_queue();
                //If bits 6/7 are high then the destination is a register
                if ((ModEncoding)((tempBL & 0xC0) >> 6) == ModEncoding.registerRegister)
                {
                    // in register-> register operations bits 3-5 of the ModRM byte encode the src
                    var srcReg = (ByteGeneral)((tempBL & 0x38) >> 3);
                    // and bits 0-2 encode the dest
                    var destReg = (ByteGeneral)(tempBL & 0x07);

                    registers[destReg] = set_flags_and_sum(registers[srcReg], registers[destReg]);
                }
                else //the destination is a memory address
                {
                    //load the address
                    build_effective_address();
                    var srcReg = (ByteGeneral)((tempBL & 0x38) >> 3);

                    byte dest = busInterfaceUnit.GetByte(overrideSegment, TempC);

                    busInterfaceUnit.SetByte(overrideSegment, TempC, set_flags_and_sum(registers[srcReg], dest));
                }
            }

            private void add_rm16_r16()
            {
                fetch_next_from_queue();
                if ((ModEncoding)((tempBL & 0xC0) >> 6) == ModEncoding.registerRegister)
                {
                    var srcReg = (WordGeneral)((tempBL & 0x38) >> 3);
                    var destReg = (WordGeneral)(tempBL & 0x07);

                    registers[destReg] = set_flags_and_sum(registers[srcReg], registers[destReg]);
                }
                else
                {
                    build_effective_address();

                    var srcReg = (WordGeneral)((tempBL & 0x38) >> 3);

                    ushort dest = (ushort)(busInterfaceUnit.GetByte(overrideSegment, TempC) << 8);
                    dest |= busInterfaceUnit.GetByte(overrideSegment, (ushort)(TempC + 1));

                    TempA = set_flags_and_sum(registers[srcReg], dest);

                    busInterfaceUnit.SetByte(overrideSegment, TempC, tempAL);
                    busInterfaceUnit.SetByte(overrideSegment, (ushort)(TempC + 1), tempAH);
                }
            }

            private void add_r8_rm8()
            {
                fetch_next_from_queue();
                //If bits 6/7 are high then the destination is a register
                if ((ModEncoding)((tempBL & 0xC0) >> 6) == ModEncoding.registerRegister)
                {
                    // in register-> register operations bits 3-5 of the ModRM byte encode the src
                    var destReg = (ByteGeneral)((tempBL & 0x38) >> 3);
                    // and bits 0-2 encode the dest
                    var srcReg = (ByteGeneral)(tempBL & 0x07);

                    byte dest = registers[destReg];
                    byte src = registers[srcReg];

                    registers[destReg] = set_flags_and_sum(registers[destReg], registers[srcReg]);
                }
                else //the destination is a memory address
                {
                    //load the address
                    build_effective_address();
                    var destReg = (ByteGeneral)((tempBL & 0x38) >> 3);

                    byte src = busInterfaceUnit.GetByte(overrideSegment, TempC);

                    registers[destReg] = set_flags_and_sum(registers[destReg], src);
                }
            }

            private void add_r16_rm16()
            {
                fetch_next_from_queue();
                //If bits 6/7 are high then the destination is a register
                if ((ModEncoding)((tempBL & 0xC0) >> 6) == ModEncoding.registerRegister)
                {
                    // in register-> register operations bits 3-5 of the ModRM byte encode the src
                    var destReg = (WordGeneral)((tempBL & 0x38) >> 3);
                    // and bits 0-2 encode the dest
                    var srcReg = (WordGeneral)(tempBL & 0x07);

                    ushort src = registers[srcReg];

                    registers[destReg] = set_flags_and_sum(registers[destReg], registers[srcReg]);
                }
                else //the destination is a memory address
                {
                    //load the address
                    build_effective_address();
                    var destReg = (WordGeneral)((tempBL & 0x38) >> 3);

                    ushort src = busInterfaceUnit.GetByte(overrideSegment, TempC);
                    src |= (ushort)(busInterfaceUnit.GetByte(overrideSegment, (ushort)(TempC + 1)) << 8);


                    registers[destReg] = set_flags_and_sum(registers[destReg], src);
                }
            }

            private void add_al_i8()
            {
                fetch_next_from_queue();
                registers.AL = set_flags_and_sum(registers.AL, tempBL);
            }

            private void add_ax_i16()
            {
                fetch_next_from_queue();
                registers.AX = set_flags_and_sum(registers.AX, TempB);
            }

            private void push_es()
            {
                //decrement stack pointer by two (x86 stacks grow downward)
                registers.SP -= 2;
                //write value of ES to SS:SP
                busInterfaceUnit.WriteSegmentToMemory(BusInterfaceUnit.Segment.ES, registers.SP, BusInterfaceUnit.Segment.SS);
            }

            private void pop_es()
            {
                //read the value at SS:SP
                //TempA = busInterfaceUnit.GetWord(BusInterfaceUnit.Segment.SS, registers.SP);
                //tell the BIU to set ES
                busInterfaceUnit.SetSegment(BusInterfaceUnit.Segment.ES, TempA);
                //increment the stack pointer by two
                registers.SP += 2;
            }

            private void or_rm8_r8()
            {
                fetch_next_from_queue();
                //If bits 6/7 are high then the destination is a register
                if ((ModEncoding)((tempBL & 0xC0) >> 6) == ModEncoding.registerRegister)
                {
                    // in register-> register operations bits 3-5 of the ModRM byte encode the src
                    var srcReg = (ByteGeneral)((tempBL & 0x38) >> 3);
                    // and bits 0-2 encode the dest
                    var destReg = (ByteGeneral)(tempBL & 0x07);

                    registers[destReg] = set_flags_and_or(registers[srcReg], registers[destReg]);
                }
                else //the destination is a memory address
                {
                    //load the address
                    build_effective_address();
                    var srcReg = (ByteGeneral)((tempBL & 0x38) >> 3);

                    byte dest = busInterfaceUnit.GetByte(overrideSegment, TempC);

                    busInterfaceUnit.SetByte(overrideSegment, TempC, set_flags_and_or(registers[srcReg], dest));
                }
            }

            private void or_rm16_r16()
            {
                fetch_next_from_queue();
                if ((ModEncoding)((tempBL & 0xC0) >> 6) == ModEncoding.registerRegister)
                {
                    var srcReg = (WordGeneral)((tempBL & 0x38) >> 3);
                    var destReg = (WordGeneral)(tempBL & 0x07);

                    registers[destReg] = set_flags_and_or(registers[srcReg], registers[destReg]);
                }
                else
                {
                    build_effective_address();

                    var srcReg = (WordGeneral)((tempBL & 0x38) >> 3);

                    ushort dest = (ushort)(busInterfaceUnit.GetByte(overrideSegment, TempC) << 8);
                    dest |= busInterfaceUnit.GetByte(overrideSegment, (ushort)(TempC + 1));

                    TempA = set_flags_and_or(registers[srcReg], dest);
                    busInterfaceUnit.SetByte(overrideSegment, TempC, tempAL);
                    busInterfaceUnit.SetByte(overrideSegment, (ushort)(TempC + 1), tempAH);
                }
            }

            private void or_r8_rm8()
            {
                fetch_next_from_queue();
                //If bits 6/7 are high then the destination is a register
                if ((ModEncoding)((tempBL & 0xC0) >> 6) == ModEncoding.registerRegister)
                {
                    // in register-> register operations bits 3-5 of the ModRM byte encode the src
                    var destReg = (ByteGeneral)((tempBL & 0x38) >> 3);
                    // and bits 0-2 encode the dest
                    var srcReg = (ByteGeneral)(tempBL & 0x07);

                    registers[destReg] = set_flags_and_or(registers[destReg], registers[srcReg]);
                }
                else //the destination is a memory address
                {
                    //load the address
                    build_effective_address();
                    var destReg = (ByteGeneral)((tempBL & 0x38) >> 3);

                    var src = busInterfaceUnit.GetByte(overrideSegment, TempC);

                    registers[destReg] = set_flags_and_or(registers[destReg], src);
                }
            }

            private void or_r16_rm16()
            {
                fetch_next_from_queue();
                //If bits 6/7 are high then the destination is a register
                if ((ModEncoding)((tempBL & 0xC0) >> 6) == ModEncoding.registerRegister)
                {
                    // in register-> register operations bits 3-5 of the ModRM byte encode the src
                    var destReg = (WordGeneral)((tempBL & 0x38) >> 3);
                    // and bits 0-2 encode the dest
                    var srcReg = (WordGeneral)(tempBL & 0x07);

                    registers[destReg] = set_flags_and_or(registers[destReg], registers[srcReg]);
                }
                else //the destination is a memory address
                {
                    //load the address
                    build_effective_address();
                    var destReg = (WordGeneral)((tempBL & 0x38) >> 3);

                    ushort src = (ushort)(busInterfaceUnit.GetByte(overrideSegment, TempC) << 8);
                    src |= busInterfaceUnit.GetByte(overrideSegment, (ushort)(TempC + 1));

                    registers[destReg] = set_flags_and_or(registers[destReg], src);
                }
            }

            private void or_al_i8()
            {
                fetch_next_from_queue();
                registers.AL = set_flags_and_or(registers.AL, tempBL);
            }

            private void or_ax_i16()
            {
                fetch_next_from_queue();
                registers.AX = set_flags_and_or(registers.AX, TempB);
            }

            private void push_cs()
            {
                //decrement stack pointer by two (x86 stacks grow downward)
                registers.SP -= 2;
                //write value of CS to SS:SP
                busInterfaceUnit.WriteSegmentToMemory(BusInterfaceUnit.Segment.CS, registers.SP, BusInterfaceUnit.Segment.SS);
            }

#if FULLFEATURE
            private void pop_cs()
            {
                //read the value at SS:SP
                TempA = busInterfaceUnit.GetWord(BusInterfaceUnit.Segment.SS, registers.SP);
                //tell the BIU to set ES
                busInterfaceUnit.SetSegment(BusInterfaceUnit.Segment.ES, TempA);
                //increment the stack pointer by two
                registers.SP += 2;
            }
#endif
        }
    }
}
