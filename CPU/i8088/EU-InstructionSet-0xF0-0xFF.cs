using System;

namespace CPU.i8088
{
    public partial class Processor
    {
        private partial class ExecutionUnit
        {
            private void lck()
            {
                busInterfaceUnit.AssertLock();
                fetch_next_from_queue();
                instructions[tempBL]?.Invoke();
                busInterfaceUnit.DeassertLock();
            }

            //0xF1 has no instruction

            private void repne_nz()
            {
                fetch_next_from_queue();

                // cmps, scas
                if (tempBL == 0xa6 || tempBL == 0xa7 || tempBL == 0xae || tempBL == 0xaf)
                {
                    repInstruction = true;
                    instructions[tempBL].Invoke();
                    registers.CX--;
                    if (registers.CX == 0 || flags.ZF)
                    {
                        repInstruction = false;
                        return;
                    }
                }
                else
                {
                    throw new InvalidOperationException();
                }
            }

            private void rep_e_nz()
            {
                if (tempBL == 0xa4 || tempBL == 0xa5 || tempBL == 0xaa || tempBL == 0xab || tempBL == 0xac || tempBL == 0xad)
                {
                    repInstruction = true;
                    instructions[tempBL].Invoke();
                    registers.CX--;
                    if (registers.CX == 0 || !flags.ZF)
                    {
                        repInstruction = false;
                        return;
                    }
                }
            }

            private void halt()
            {
                throw new NotImplementedException();
            }

            private void cmc()
            {
                flags.CF ^= true;
            }

            private void oxf6_rm8()
            {
                fetch_next_from_queue();
                if ((ModEncoding)((tempBL & 0xc0) >> 6) == ModEncoding.registerRegister)
                {
                    var destReg = (ByteGeneral)(tempBL & 0x07);
                    tempAL = registers[destReg];
                }
                else
                {
                    build_effective_address();
                    tempAL = busInterfaceUnit.GetByte(overrideSegment, TempC);
                }
                switch ((byte)((tempBL & 0x38) >> 3))
                {
                    case 0: // test_rm8_imm8
                        fetch_next_from_queue();
                        set_flags_and_and(tempAL, tempBL);
                        return;
                    case 1: // nc
                        return;
                    case 2: // not_rm8
                        registers.AL = (byte)(tempCL ^ 0xff);
                        return;
                    case 3: // neg_rm8
                        if (tempAL == 0x80)
                        {
                            flags.OF = true;
                            return;
                        }
                        registers.AL = set_flags_and_diff(0, tempAL);
                        return;
                    case 4: // mul_rm8
                        registers.AX = (ushort)(registers.AL * tempAL);
                        flags.CF = flags.OF = registers.AH != 0;
                        return;
                    case 5: // imul_rm8
                        registers.AX = set_flags_and_imul(registers.AL, tempAL);
                        return;
                    case 6: // div_rm8
                        if (registers.AX / tempAL > 0xff)
                        {
                            //INT 0
                        }
                        tempCL = (byte)(registers.AX / tempAL);
                        tempCH = (byte)(registers.AX % tempAL);
                        registers.AX = TempC;
                        return;
                    case 7: // idiv_rm8
                        if ((short)registers.AX / (sbyte)tempAL > 127 || (short)registers.AX / (sbyte)tempAL < -127)
                        {
                            //INT 0
                        }
                        tempCL = (byte)((short)registers.AX / (sbyte)tempAL);
                        tempCH = (byte)((short)registers.AX % (sbyte)tempAL);
                        registers.AX = TempC;
                        return;
                    default:
                        throw new InvalidOperationException();
                }
            }

            private void oxf7_rm16()
            {
                fetch_next_from_queue();
                if ((ModEncoding)((tempBL & 0xc0) >> 6) == ModEncoding.registerRegister)
                {
                    var destReg = (WordGeneral)(tempBL & 0x07);
                    TempA = registers[destReg];
                }
                else
                {
                    build_effective_address();
                    TempA = busInterfaceUnit.GetWord(overrideSegment, TempC);
                }
                switch ((byte)((tempBL & 0x38) >> 3))
                {
                    case 0: // test_rm16_imm16
                        fetch_next_from_queue();
                        tempCL = tempBL;
                        fetch_next_from_queue();
                        tempCH = tempBL;
                        set_flags_and_and(TempA, TempC);
                        return;
                    case 1: // nc
                        return;
                    case 2: // not_rm16
                        registers.AX = (ushort)(TempC ^ 0xffff);
                        return;
                    case 3: // neg_rm16
                        if (TempA == 0x8000)
                        {
                            flags.OF = true;
                            return;
                        }
                        registers.AX = set_flags_and_diff(0, TempA);
                        return;
                    case 4: // mul_rm16
                        uint result = (uint)registers.AX * TempA;
                        flags.CF = flags.OF = (result & 0xffff0000) != 0;
                        registers.DX = (ushort)((result & 0xffff0000) >> 16);
                        registers.AX = (ushort)(result & 0xffff);
                        return;
                    case 5: // imul_rm16
                        int iresult = registers.AX * TempA;
                        uint signtest = (ushort)(iresult & 0xffff);
                        signtest |= (((signtest & 0x8000) != 0) ? 0xffff0000 : 0);
                        flags.CF = flags.OF = iresult == signtest;
                        registers.DX = (ushort)((iresult & 0xffff0000) >> 16);
                        registers.AX = (ushort)(iresult & 0xffff);
                        break;
                    case 6: // div_rm8
                        uint operand = (uint)((registers.DX << 16) | registers.AX);
                        if (operand / TempB > 0xffff)
                        {
                            //INT 0
                        }
                        registers.DX = (ushort)(operand % TempA);
                        registers.AX = (ushort)(operand / TempA);
                        return;
                    case 7: // idiv_rm8
                        int ioperand = (registers.DX << 16) | registers.AX;
                        if (ioperand / TempA > 32767 || ioperand / TempA > -32767)
                        {
                            //INT 0
                        }
                        registers.DX = (ushort)(ioperand % TempA);
                        registers.AX = (ushort)(ioperand / TempA);
                        return;
                    default:
                        throw new InvalidOperationException();
                }
            }

            private void clc()
            {
                flags.CF = false;
            }

            private void stc()
            {
                flags.CF = true;
            }

            private void cli()
            {
                flags.IF = false;
            }

            private void sti()
            {
                flags.IF = true;
            }

            private void cld()
            {
                flags.DF = false;
            }

            private void std()
            {
                flags.DF = true;
            }

            private void inc_dec_rm8()
            {
                fetch_next_from_queue();
                var op = (byte)((tempBL & 0x38) >> 3);
                if((ModEncoding)((tempBL & 0xc0) >> 6) == ModEncoding.registerRegister)
                {
                    var destReg = (ByteGeneral)(tempBL & 0x07);
                    registers[destReg] = op == 0 ? (byte)(registers[destReg] + 1) : (byte)(registers[destReg] - 1);
                }
                else
                {
                    build_effective_address();
                    var dest = busInterfaceUnit.GetByte(overrideSegment, TempC);
                    dest = op == 0 ? (byte)(dest + 1) : (byte)(dest - 1);
                    busInterfaceUnit.SetByte(overrideSegment, TempC, dest);
                }
            }

            private void oxff()
            {
                fetch_next_from_queue();
                var op = (byte)((tempBL & 0x38) >> 3);
                if((ModEncoding)((tempBL & 0xc0) >> 6) == ModEncoding.registerRegister)
                {
                    var destReg = (WordGeneral)(tempBL & 0x07);
                    var dest = registers[destReg];
                }
            }
        }
    }
}
