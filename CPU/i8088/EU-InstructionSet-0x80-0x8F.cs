using System;

namespace CPU.i8088
{
    public partial class Processor
    {
        private partial class ExecutionUnit
        {
            private void o80h_rm8_i8()
            {
                fetch_next_from_queue();
                switch ((tempBL & 0x38) >> 3)
                {
                    case 0:
                        add_rm8_i8();
                        break;
                    case 1:
                        or_rm8_i8();
                        break;
                    case 2:
                        adc_rm8_i8();
                        break;
                    case 3:
                        sbb_rm8_i8();
                        break;
                    case 4:
                        and_rm8_i8();
                        break;
                    case 5:
                        sub_rm8_i8();
                        break;
                    case 6:
                        xor_rm8_i8();
                        break;
                    case 7:
                        cmp_rm8_i8();
                        break;
                    default:
                        throw new InvalidOperationException();
                }
            }

            private void add_rm8_i8()
            {
                if ((ModEncoding)((tempBL & 0xC0) >> 6) == ModEncoding.registerRegister)
                {
                    var destReg = (ByteGeneral)(tempBL & 0x07);

                    fetch_next_from_queue();

                    registers[destReg] = set_flags_and_sum(registers[destReg], tempBL);
                }
                else
                {
                    build_effective_address();

                    byte dest = busInterfaceUnit.GetByte(overrideSegment, TempC);

                    fetch_next_from_queue();

                    busInterfaceUnit.SetByte(overrideSegment, TempC, set_flags_and_sum(dest, tempBL));
                }
            }

            private void or_rm8_i8()
            {
                if ((ModEncoding)((tempBL & 0xC0) >> 6) == ModEncoding.registerRegister)
                {
                    var destReg = (ByteGeneral)(tempBL & 0x07);

                    fetch_next_from_queue();

                    registers[destReg] = set_flags_and_or(registers[destReg], tempBL);
                }
                else
                {
                    build_effective_address();

                    byte dest = busInterfaceUnit.GetByte(overrideSegment, TempC);

                    fetch_next_from_queue();

                    busInterfaceUnit.SetByte(overrideSegment, TempC, set_flags_and_or(dest, tempBL));
                }
            }

            private void adc_rm8_i8()
            {
                if ((ModEncoding)((tempBL & 0xC0) >> 6) == ModEncoding.registerRegister)
                {
                    var destReg = (ByteGeneral)(tempBL & 0x07);

                    fetch_next_from_queue();

                    registers[destReg] = set_flags_and_sum_carry(registers[destReg], tempBL);
                }
                else
                {
                    build_effective_address();

                    byte dest = busInterfaceUnit.GetByte(overrideSegment, TempC);

                    fetch_next_from_queue();

                    busInterfaceUnit.SetByte(overrideSegment, TempC, set_flags_and_sum_carry(dest, tempBL));
                }
            }

            private void sbb_rm8_i8()
            {
                if ((ModEncoding)((tempBL & 0xC0) >> 6) == ModEncoding.registerRegister)
                {
                    var destReg = (ByteGeneral)(tempBL & 0x07);

                    fetch_next_from_queue();

                    registers[destReg] = set_flags_and_diff_borrow(registers[destReg], tempBL);
                }
                else
                {
                    build_effective_address();

                    byte dest = busInterfaceUnit.GetByte(overrideSegment, TempC);

                    fetch_next_from_queue();

                    busInterfaceUnit.SetByte(overrideSegment, TempC, set_flags_and_diff_borrow(dest, tempBL));
                }
            }

            private void and_rm8_i8()
            {
                if ((ModEncoding)((tempBL & 0xC0) >> 6) == ModEncoding.registerRegister)
                {
                    var destReg = (ByteGeneral)(tempBL & 0x07);

                    fetch_next_from_queue();

                    registers[destReg] = set_flags_and_and(registers[destReg], tempBL);
                }
                else
                {
                    build_effective_address();

                    byte dest = busInterfaceUnit.GetByte(overrideSegment, TempC);

                    fetch_next_from_queue();

                    busInterfaceUnit.SetByte(overrideSegment, TempC, set_flags_and_and(dest, tempBL));
                }
            }

            private void sub_rm8_i8()
            {
                if ((ModEncoding)((tempBL & 0xC0) >> 6) == ModEncoding.registerRegister)
                {
                    var destReg = (ByteGeneral)(tempBL & 0x07);

                    fetch_next_from_queue();

                    registers[destReg] = set_flags_and_diff(registers[destReg], tempBL);
                }
                else
                {
                    build_effective_address();

                    byte dest = busInterfaceUnit.GetByte(overrideSegment, TempC);

                    fetch_next_from_queue();

                    busInterfaceUnit.SetByte(overrideSegment, TempC, set_flags_and_diff(dest, tempBL));
                }
            }

            private void xor_rm8_i8()
            {
                if ((ModEncoding)((tempBL & 0xC0) >> 6) == ModEncoding.registerRegister)
                {
                    var destReg = (ByteGeneral)(tempBL & 0x07);

                    fetch_next_from_queue();

                    registers[destReg] = set_flags_and_xor(registers[destReg], tempBL);
                }
                else
                {
                    build_effective_address();

                    byte dest = busInterfaceUnit.GetByte(overrideSegment, TempC);

                    fetch_next_from_queue();

                    busInterfaceUnit.SetByte(overrideSegment, TempC, set_flags_and_xor(dest, tempBL));
                }
            }

            private void cmp_rm8_i8()
            {
                if ((ModEncoding)((tempBL & 0xC0) >> 6) == ModEncoding.registerRegister)
                {
                    var destReg = (ByteGeneral)(tempBL & 0x07);

                    fetch_next_from_queue();

                    tempAL = set_flags_and_diff(registers[destReg], tempBL);
                }
                else
                {
                    build_effective_address();

                    byte dest = busInterfaceUnit.GetByte(overrideSegment, TempC);

                    fetch_next_from_queue();

                    tempAL = set_flags_and_sum(dest, tempBL);
                }
            }

            private void o81h_rm16_i16()
            {
                fetch_next_from_queue();
                switch ((tempBL & 0x38) >> 3)
                {
                    case 0:
                        add_rm16_i16();
                        break;
                    case 1:
                        or_rm16_i16();
                        break;
                    case 2:
                        adc_rm16_i16();
                        break;
                    case 3:
                        sbb_rm16_i16();
                        break;
                    case 4:
                        and_rm16_i16();
                        break;
                    case 5:
                        sub_rm16_i16();
                        break;
                    case 6:
                        xor_rm16_i16();
                        break;
                    case 7:
                        cmp_rm16_i16();
                        break;
                    default:
                        throw new InvalidOperationException();
                }
            }

            private void add_rm16_i16()
            {
                if ((ModEncoding)((tempBL & 0xC0) >> 6) == ModEncoding.registerRegister)
                {
                    var destReg = (WordGeneral)(tempBL & 0x07);

                    fetch_next_from_queue();
                    tempAL = tempBL;

                    fetch_next_from_queue();

                    tempAH = tempBL;

                    registers[destReg] = set_flags_and_sum(registers[destReg], TempA);
                }
                else
                {
                    build_effective_address();

                    ushort dest = busInterfaceUnit.GetWord(overrideSegment, TempC);

                    fetch_next_from_queue();

                    tempAL = tempBL;

                    fetch_next_from_queue();

                    tempAH = tempBL;

                    busInterfaceUnit.SetWord(overrideSegment, TempC, set_flags_and_sum(dest, TempA));
                }
            }

            private void or_rm16_i16()
            {
                if ((ModEncoding)((tempBL & 0xC0) >> 6) == ModEncoding.registerRegister)
                {
                    var destReg = (WordGeneral)(tempBL & 0x07);

                    fetch_next_from_queue();
                    tempAL = tempBL;

                    fetch_next_from_queue();

                    TempA |= (ushort)(tempBL << 8);

                    registers[destReg] = set_flags_and_or(registers[destReg], TempA);
                }
                else
                {
                    build_effective_address();

                    ushort dest = busInterfaceUnit.GetWord(overrideSegment, TempC);

                    fetch_next_from_queue();

                    tempAL = tempBL;

                    fetch_next_from_queue();

                    tempAH = tempBL;

                    busInterfaceUnit.SetWord(overrideSegment, TempC, set_flags_and_or(dest, TempA));
                }
            }

            private void adc_rm16_i16()
            {
                if ((ModEncoding)((tempBL & 0xC0) >> 6) == ModEncoding.registerRegister)
                {
                    var destReg = (WordGeneral)(tempBL & 0x07);

                    fetch_next_from_queue();
                    tempAL = tempBL;

                    fetch_next_from_queue();

                    TempA |= (ushort)(tempBL << 8);

                    registers[destReg] = set_flags_and_sum_carry(registers[destReg], TempA);
                }
                else
                {
                    build_effective_address();

                    ushort dest = busInterfaceUnit.GetWord(overrideSegment, TempC);

                    fetch_next_from_queue();

                    tempAL = tempBL;

                    fetch_next_from_queue();

                    tempAH = tempBL;

                    busInterfaceUnit.SetWord(overrideSegment, TempC, set_flags_and_sum_carry(dest, TempA));
                }
            }

            private void sbb_rm16_i16()
            {
                if ((ModEncoding)((tempBL & 0xC0) >> 6) == ModEncoding.registerRegister)
                {
                    var destReg = (WordGeneral)(tempBL & 0x07);

                    fetch_next_from_queue();
                    tempAL = tempBL;

                    fetch_next_from_queue();

                    TempA |= (ushort)(tempBL << 8);

                    registers[destReg] = set_flags_and_diff_borrow(registers[destReg], TempA);
                }
                else
                {
                    build_effective_address();

                    ushort dest = busInterfaceUnit.GetWord(overrideSegment, TempC);

                    fetch_next_from_queue();

                    tempAL = tempBL;

                    fetch_next_from_queue();

                    tempAH = tempBL;

                    busInterfaceUnit.SetWord(overrideSegment, TempC, set_flags_and_diff_borrow(dest, TempA));
                }
            }

            private void and_rm16_i16()
            {
                if ((ModEncoding)((tempBL & 0xC0) >> 6) == ModEncoding.registerRegister)
                {
                    var destReg = (WordGeneral)(tempBL & 0x07);

                    fetch_next_from_queue();
                    tempAL = tempBL;

                    fetch_next_from_queue();

                    TempA |= (ushort)(tempBL << 8);

                    registers[destReg] = set_flags_and_and(registers[destReg], TempA);
                }
                else
                {
                    build_effective_address();

                    ushort dest = busInterfaceUnit.GetWord(overrideSegment, TempC);

                    fetch_next_from_queue();

                    tempAL = tempBL;

                    fetch_next_from_queue();

                    tempAH = tempBL;

                    busInterfaceUnit.SetWord(overrideSegment, TempC, set_flags_and_and(dest, TempA));
                }
            }

            private void sub_rm16_i16()
            {
                if ((ModEncoding)((tempBL & 0xC0) >> 6) == ModEncoding.registerRegister)
                {
                    var destReg = (WordGeneral)(tempBL & 0x07);

                    fetch_next_from_queue();
                    tempAL = tempBL;

                    fetch_next_from_queue();

                    TempA |= (ushort)(tempBL << 8);

                    registers[destReg] = set_flags_and_diff(registers[destReg], TempA);
                }
                else
                {
                    build_effective_address();

                    ushort dest = busInterfaceUnit.GetWord(overrideSegment, TempC);

                    fetch_next_from_queue();

                    tempAL = tempBL;

                    fetch_next_from_queue();

                    tempAH = tempBL;

                    busInterfaceUnit.SetWord(overrideSegment, TempC, set_flags_and_diff(dest, TempA));
                }
            }

            private void xor_rm16_i16()
            {
                if ((ModEncoding)((tempBL & 0xC0) >> 6) == ModEncoding.registerRegister)
                {
                    var destReg = (WordGeneral)(tempBL & 0x07);

                    fetch_next_from_queue();
                    tempAL = tempBL;

                    fetch_next_from_queue();

                    TempA |= (ushort)(tempBL << 8);

                    registers[destReg] = set_flags_and_xor(registers[destReg], TempA);
                }
                else
                {
                    build_effective_address();

                    ushort dest = busInterfaceUnit.GetWord(overrideSegment, TempC);

                    fetch_next_from_queue();

                    tempAL = tempBL;

                    fetch_next_from_queue();

                    tempAH = tempBL;

                    busInterfaceUnit.SetWord(overrideSegment, TempC, set_flags_and_xor(dest, TempA));
                }
            }

            private void cmp_rm16_i16()
            {
                if ((ModEncoding)((tempBL & 0xC0) >> 6) == ModEncoding.registerRegister)
                {
                    var destReg = (WordGeneral)(tempBL & 0x07);

                    fetch_next_from_queue();
                    tempAL = tempBL;

                    fetch_next_from_queue();

                    TempA |= (ushort)(tempBL << 8);

                    TempC = set_flags_and_diff(registers[destReg], TempA);
                }
                else
                {
                    build_effective_address();

                    ushort dest = busInterfaceUnit.GetWord(overrideSegment, TempC);

                    fetch_next_from_queue();

                    tempAL = tempBL;

                    fetch_next_from_queue();

                    tempAH = tempBL;

                    TempC = set_flags_and_diff(dest, TempA);
                }
            }

            private void o82h_rm8_i16()
            {
                fetch_next_from_queue();
                switch ((tempBL & 0x38) >> 3)
                {
                    case 0:
                        add_rm8_i8();
                        break;
                    case 1:
                        break;
                    case 2:
                        adc_rm8_i8();
                        break;
                    case 3:
                        sbb_rm8_i8();
                        break;
                    case 4:
                        break;
                    case 5:
                        sub_rm8_i8();
                        break;
                    case 6:
                        break;
                    case 7:
                        cmp_rm8_i8();
                        break;
                    default:
                        throw new InvalidOperationException();
                }
            }

            private void o83h_rm16_i16()
            {
                fetch_next_from_queue();
                switch ((tempBL & 0x38) >> 3)
                {
                    case 0:
                        add_rm16_i16();
                        break;
                    case 1:
                        break;
                    case 2:
                        adc_rm16_i16();
                        break;
                    case 3:
                        sbb_rm16_i16();
                        break;
                    case 4:
                        break;
                    case 5:
                        sub_rm16_i16();
                        break;
                    case 6:
                        break;
                    case 7:
                        cmp_rm16_i16();
                        break;
                    default:
                        throw new InvalidOperationException();
                }
            }

            private void test_rm8_r8()
            {
                fetch_next_from_queue();
                //If bits 6/7 are high then the destination is a register
                if ((ModEncoding)((tempBL & 0xC0) >> 6) == ModEncoding.registerRegister)
                {
                    // in register-> register operations bits 3-5 of the ModRM byte encode the src
                    var srcReg = (ByteGeneral)((tempBL & 0x38) >> 3);
                    // and bits 0-2 encode the dest
                    var destReg = (ByteGeneral)(tempBL & 0x07);

                    tempAL = set_flags_and_and(registers[destReg], registers[srcReg]);
                }
                else //the destination is a memory address
                {
                    //load the address
                    var srcReg = (ByteGeneral)((tempBL & 0x38) >> 3);
                    build_effective_address();

                    byte dest = busInterfaceUnit.GetByte(overrideSegment, TempC);

                    tempAL = set_flags_and_and(dest, registers[srcReg]);
                }
            }

            private void test_rm16_r16()
            {
                fetch_next_from_queue();
                if ((ModEncoding)((tempBL & 0xC0) >> 6) == ModEncoding.registerRegister)
                {
                    var srcReg = (WordGeneral)((tempBL & 0x38) >> 3);
                    var destReg = (WordGeneral)(tempBL & 0x07);

                    TempA = set_flags_and_and(registers[destReg], registers[srcReg]);
                }
                else
                {
                    var srcReg = (WordGeneral)((tempBL & 0x38) >> 3);
                    build_effective_address();

                    ushort dest = busInterfaceUnit.GetWord(overrideSegment, TempC);

                    TempA = set_flags_and_and(registers[srcReg], dest);

                    TempA = set_flags_and_and(dest, registers[srcReg]);
                }
            }

            private void xchg_rm8_r8()
            {
                fetch_next_from_queue();
                if ((ModEncoding)((tempBL & 0xC0) >> 6) == ModEncoding.registerRegister)
                {
                    var srcReg = (ByteGeneral)((tempBL & 0x38) >> 3);
                    var destReg = (ByteGeneral)(tempBL & 0x07);

                    tempAL = registers[srcReg];
                    registers[srcReg] = registers[destReg];
                    registers[destReg] = tempAL;
                }
                else
                {
                    var srcReg = (ByteGeneral)((tempBL & 0x38) >> 3);
                    build_effective_address();

                    byte dest = busInterfaceUnit.GetByte(overrideSegment, TempC);

                    tempAL = registers[srcReg];
                    registers[srcReg] = dest;
                    busInterfaceUnit.SetByte(overrideSegment, TempC, tempAL);
                }
            }

            private void xchg_rm16_r16()
            {
                fetch_next_from_queue();
                if ((ModEncoding)((tempBL & 0xC0) >> 6) == ModEncoding.registerRegister)
                {
                    var srcReg = (WordGeneral)((tempBL & 0x38) >> 3);
                    var destReg = (WordGeneral)(tempBL & 0x07);

                    TempA = registers[srcReg];
                    registers[srcReg] = registers[destReg];
                    registers[destReg] = TempA;
                }
                else
                {
                    var srcReg = (WordGeneral)((tempBL & 0x38) >> 3);
                    build_effective_address();

                    ushort dest = busInterfaceUnit.GetByte(overrideSegment, TempC);

                    TempA = registers[srcReg];
                    registers[srcReg] = dest;
                    busInterfaceUnit.SetWord(overrideSegment, TempC, TempA);
                }
            }

            private void mov_rm8_r8()
            {
                fetch_next_from_queue();
                if ((ModEncoding)((tempBL & 0xc0) >> 6) == ModEncoding.registerRegister)
                {
                    var srcReg = (ByteGeneral)((tempBL & 0x38) >> 3);
                    var destReg = (ByteGeneral)(tempBL & 0x07);

                    registers[destReg] = registers[srcReg];
                }
                else
                {
                    var srcReg = (ByteGeneral)((tempBL & 0x38) >> 3);

                    build_effective_address();

                    busInterfaceUnit.SetByte(overrideSegment, TempC, registers[srcReg]);
                }
            }

            private void mov_rm16_r16()
            {
                fetch_next_from_queue();
                if ((ModEncoding)((tempBL & 0xc0) >> 6) == ModEncoding.registerRegister)
                {
                    var srcReg = (WordGeneral)((tempBL & 0x38) >> 3);
                    var destReg = (WordGeneral)(tempBL & 0x07);

                    registers[destReg] = registers[srcReg];
                }
                else
                {
                    var srcReg = (WordGeneral)((tempBL & 0x38) >> 3);

                    build_effective_address();

                    busInterfaceUnit.SetWord(overrideSegment, TempC, registers[srcReg]);
                }
            }

            private void mov_r8_rm8()
            {
                fetch_next_from_queue();
                if ((ModEncoding)((tempBL & 0xc0) >> 6) == ModEncoding.registerRegister)
                {
                    var destReg = (ByteGeneral)((tempBL & 0x38) >> 3);
                    var srcReg = (ByteGeneral)(tempBL & 0x07);

                    registers[destReg] = registers[srcReg];
                }
                else
                {
                    var destReg = (ByteGeneral)((tempBL & 0x38) >> 3);

                    build_effective_address();

                    registers[destReg] = busInterfaceUnit.GetByte(overrideSegment, TempC);
                }
            }

            private void mov_r16_rm16()
            {
                fetch_next_from_queue();
                if((ModEncoding)((tempBL & 0xc0) >> 6) == ModEncoding.registerRegister)
                {
                    var destReg = (WordGeneral)((tempBL & 0x38) >> 3);
                    var srcReg = (WordGeneral)(tempBL & 0x07);

                    registers[destReg] = registers[srcReg];
                }
                else
                {
                    var srcReg = (WordGeneral)((tempBL & 0x38) >> 3);

                    build_effective_address();

                    busInterfaceUnit.SetWord(overrideSegment, TempC, registers[srcReg]);
                }
            }

            private void mov_rm16_seg()
            {
                fetch_next_from_queue();
                if((ModEncoding)((tempBL & 0xc0) >> 6) == ModEncoding.registerRegister)
                {
                    var srcReg = (BusInterfaceUnit.Segment)((tempBL & 0x18) >> 3);
                    var destReg = (WordGeneral)(tempBL & 0x07);

                    registers[destReg] = busInterfaceUnit.GetSegment(srcReg);
                }
            }

            private void lea_r16_m16()
            {
                fetch_next_from_queue();
                if ((ModEncoding)((tempBL & 0xc0) >> 6) == ModEncoding.registerRegister)
                    throw new InvalidOperationException();

                var destReg = (WordGeneral)(tempBL & 0x07);

                build_effective_address();

                registers[destReg] = TempC;
            }

            private void mov_seg_rm16()
            {
                fetch_next_from_queue();
                if ((ModEncoding)((tempBL & 0xc0) >> 6) == ModEncoding.registerRegister)
                {
                    var srcReg = (WordGeneral)(tempBL & 0x07);
                    var destReg = (BusInterfaceUnit.Segment)((tempBL & 0x38) >> 3);

                    busInterfaceUnit.SetSegment(destReg, registers[srcReg]);
                }
                else
                {
                    var destReg = (BusInterfaceUnit.Segment)((tempBL & 0x38) >> 3);

                    build_effective_address();

                    busInterfaceUnit.GetWord(overrideSegment, TempC);

                    busInterfaceUnit.SetSegment(destReg, TempB);
                }
            }

            private void pop_rm16()
            {
                fetch_next_from_queue();

                if ((tempBL & 0xc0) != 0)
                    throw new InvalidOperationException();

                build_effective_address();

                TempA = busInterfaceUnit.GetWord(BusInterfaceUnit.Segment.SS, registers.SP);
                registers.SP += 2;
                busInterfaceUnit.SetWord(overrideSegment, TempC, TempA);
            }
        }
    }
}
