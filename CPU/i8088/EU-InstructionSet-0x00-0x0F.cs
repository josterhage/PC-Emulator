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
                    var src = (ByteGeneral)(tempBL & 0x07);
                    // and bits 0-2 encode the dest
                    var dest = (ByteGeneral)((tempBL & 0x38) >> 3);

                    registers[dest] += registers[src];
                }
                else //the destination is a memory address
                {
                    //load the address
                    build_effective_address();
                    var src = (ByteGeneral)(tempBL & 0x07);

                    var destVal = busInterfaceUnit.GetByte(overrideSegment, TempC);

                    destVal += registers[src];

                    busInterfaceUnit.SetByte(overrideSegment, TempC, destVal);
                }
            }

            private void add_rm16_r16()
            {
                fetch_next_from_queue();
                if ((ModEncoding)((tempBL & 0xC0) >> 6) == ModEncoding.registerRegister)
                {
                    var src = (WordGeneral)(tempBL & 0x07);
                    var dest = (WordGeneral)((tempBL & 0x38) >> 3);

                    registers[dest] += registers[src];
                }
                else
                {
                    build_effective_address();

                    var src = (WordGeneral)(tempBL & 0x07);

                    var destVal = busInterfaceUnit.GetWord(overrideSegment, TempC);

                    destVal += registers[src];

                    busInterfaceUnit.SetWord(overrideSegment, TempC, destVal);
                }
            }

            private void add_r8_rm8()
            {
                fetch_next_from_queue();
                //If bits 6/7 are high then the destination is a register
                if ((ModEncoding)((tempBL & 0xC0) >> 6) == ModEncoding.registerRegister)
                {
                    // in register-> register operations bits 3-5 of the ModRM byte encode the src
                    var dest = (ByteGeneral)(tempBL & 0x07);
                    // and bits 0-2 encode the dest
                    var src = (ByteGeneral)((tempBL & 0x38) >> 3);

                    registers[dest] += registers[src];
                }
                else //the destination is a memory address
                {
                    //load the address
                    build_effective_address();
                    var dest = (ByteGeneral)(tempBL & 0x07);

                    var srcVal = busInterfaceUnit.GetByte(overrideSegment, TempC);

                    registers[dest] += srcVal;
                }
            }

            private void add_r16_rm16()
            {
                fetch_next_from_queue();
                //If bits 6/7 are high then the destination is a register
                if ((ModEncoding)((tempBL & 0xC0) >> 6) == ModEncoding.registerRegister)
                {
                    // in register-> register operations bits 3-5 of the ModRM byte encode the src
                    var dest = (WordGeneral)(tempBL & 0x07);
                    // and bits 0-2 encode the dest
                    var src = (WordGeneral)((tempBL & 0x38) >> 3);

                    registers[dest] += registers[src];
                }
                else //the destination is a memory address
                {
                    //load the address
                    build_effective_address();
                    var dest = (WordGeneral)(tempBL & 0x07);

                    var srcVal = busInterfaceUnit.GetWord(overrideSegment, TempC);

                    registers[dest] += srcVal;
                }
            }

            private void add_al_i8()
            {
                fetch_next_from_queue();
                registers.AL += tempBL;
            }

            private void add_ax_i16()
            {
                fetch_next_from_queue();
                registers.AX += TempB;
            }

            private void push_es()
            {
                //write value of ES to SS:SP
                busInterfaceUnit.WriteSegmentToMemory(BusInterfaceUnit.Segment.ES, registers.SP, BusInterfaceUnit.Segment.SS);
                //decrement stack pointer by two (x86 stacks grow downward)
                registers.SP -= 2;
            }

            private void pop_es()
            {
                //STACK UNDERFLOW - if these are equal that means there is no data on the stack
                //TODO: check what the 8086 does
                //if(registers.SP == registers.BP)
                //{
                //    throw new InvalidOperationException();
                //}

                //read the value at SS:SP
                TempA = busInterfaceUnit.GetWord(BusInterfaceUnit.Segment.SS, registers.SP);
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
                    var src = (ByteGeneral)(tempBL & 0x07);
                    // and bits 0-2 encode the dest
                    var dest = (ByteGeneral)((tempBL & 0x38) >> 3);

                    registers[dest] |= registers[src];
                }
                else //the destination is a memory address
                {
                    //load the address
                    build_effective_address();
                    var src = (ByteGeneral)(tempBL & 0x07);

                    var destVal = busInterfaceUnit.GetByte(overrideSegment, TempC);

                    destVal |= registers[src];

                    busInterfaceUnit.SetByte(overrideSegment, TempC, destVal);
                }
            }

            private void or_rm16_r16()
            {
                fetch_next_from_queue();
                if ((ModEncoding)((tempBL & 0xC0) >> 6) == ModEncoding.registerRegister)
                {
                    var src = (WordGeneral)(tempBL & 0x07);
                    var dest = (WordGeneral)((tempBL & 0x38) >> 3);

                    registers[dest] |= registers[src];
                }
                else
                {
                    build_effective_address();

                    var src = (WordGeneral)(tempBL & 0x07);

                    var destVal = busInterfaceUnit.GetWord(overrideSegment, TempC);

                    destVal |= registers[src];

                    busInterfaceUnit.SetWord(overrideSegment, TempC, destVal);
                }
            }

            private void or_r8_rm16()
            {
                fetch_next_from_queue();
                //If bits 6/7 are high then the destination is a register
                if ((ModEncoding)((tempBL & 0xC0) >> 6) == ModEncoding.registerRegister)
                {
                    // in register-> register operations bits 3-5 of the ModRM byte encode the src
                    var dest = (ByteGeneral)(tempBL & 0x07);
                    // and bits 0-2 encode the dest
                    var src = (ByteGeneral)((tempBL & 0x38) >> 3);

                    registers[dest] |= registers[src];
                }
                else //the destination is a memory address
                {
                    //load the address
                    build_effective_address();
                    var dest = (ByteGeneral)(tempBL & 0x07);

                    var srcVal = busInterfaceUnit.GetByte(overrideSegment, TempC);

                    registers[dest] |= srcVal;
                }
            }

            private void or_r16_rm16()
            {
                fetch_next_from_queue();
                //If bits 6/7 are high then the destination is a register
                if ((ModEncoding)((tempBL & 0xC0) >> 6) == ModEncoding.registerRegister)
                {
                    // in register-> register operations bits 3-5 of the ModRM byte encode the src
                    var dest = (WordGeneral)(tempBL & 0x07);
                    // and bits 0-2 encode the dest
                    var src = (WordGeneral)((tempBL & 0x38) >> 3);

                    registers[dest] |= registers[src];
                }
                else //the destination is a memory address
                {
                    //load the address
                    build_effective_address();
                    var dest = (WordGeneral)(tempBL & 0x07);

                    var srcVal = busInterfaceUnit.GetWord(overrideSegment, TempC);

                    registers[dest] |= srcVal;
                }
            }

            private void or_al_i8()
            {
                fetch_next_from_queue();
                registers.AL |= tempBL;
            }

            private void or_ax_i16()
            {
                fetch_next_from_queue();
                registers.AX |= TempB;
            }

            private void push_cs()
            {
                //write value of CS to SS:SP
                busInterfaceUnit.WriteSegmentToMemory(BusInterfaceUnit.Segment.CS, registers.SP, BusInterfaceUnit.Segment.SS);
                //decrement stack pointer by two (x86 stacks grow downward)
                registers.SP -= 2;
            }

#if FULLFEATURE
            private void pop_cs()
            {
                //STACK UNDERFLOW - if these are equal that means there is no data on the stack
                //TODO: check what the 8086 does
                //if(registers.SP == registers.BP)
                //{
                //    throw new InvalidOperationException();
                //}

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
