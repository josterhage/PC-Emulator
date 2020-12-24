using System;

namespace CPU.i8088
{
    public partial class Processor
    {
        private partial class ExecutionUnit
        {
            private void rot_shift1_rm8()
            {
                fetch_next_from_queue();
                if ((ModEncoding)((tempBL & 0xc0) >> 6) == ModEncoding.registerRegister)
                {
                    var destReg = (ByteGeneral)(tempBL & 0x07);

                    tempAL = registers[destReg];

                    switch ((byte)((tempBL & 0x38) >> 3))
                    {
                        case 0: //rol
                            registers[destReg] = rol(tempAL);
                            break;
                        case 1: //ror
                            registers[destReg] = ror(tempAL);
                            break;
                        case 2: //rcl
                            registers[destReg] = rcl(tempAL);
                            break;
                        case 3: //rcr
                            registers[destReg] = rcr(tempAL);
                            break;
                        case 4: //sal/shl
                            registers[destReg] = sal(tempAL);
                            break;
                        case 5: //shr
                            registers[destReg] = shr(tempAL);
                            break;
                        case 6: //NC
                            break;
                        case 7: //SAR
                            registers[destReg] = sar(tempAL);
                            break;
                        default:
                            throw new InvalidOperationException();
                    }
                }
                else
                {
                    build_effective_address();

                    tempAL = busInterfaceUnit.GetByte(overrideSegment, TempC);

                    switch ((byte)((tempBL & 0x38) >> 3))
                    {
                        case 0: //rol
                            busInterfaceUnit.SetByte(overrideSegment,TempC,rol(tempAL));
                            break;
                        case 1: //ror
                            busInterfaceUnit.SetByte(overrideSegment, TempC, ror(tempAL));
                            break;
                        case 2: //rcl
                            busInterfaceUnit.SetByte(overrideSegment, TempC, rcl(tempAL));
                            break;
                        case 3: //rcr
                            busInterfaceUnit.SetByte(overrideSegment, TempC, rcr(tempAL));
                            break;
                        case 4: //sal/shl
                            busInterfaceUnit.SetByte(overrideSegment, TempC, sal(tempAL));
                            break;
                        case 5: //shr
                            busInterfaceUnit.SetByte(overrideSegment, TempC, shr(tempAL));
                            break;
                        case 6: //NC
                            break;
                        case 7: //SAR
                            busInterfaceUnit.SetByte(overrideSegment, TempC, sar(tempAL));
                            break;
                        default:
                            throw new InvalidOperationException();
                    }
                }
            }

            private void rot_shift1_rm16()
            {
                fetch_next_from_queue();
                if ((ModEncoding)((tempBL & 0xc0) >> 6) == ModEncoding.registerRegister)
                {
                    var destReg = (WordGeneral)(tempBL & 0x07);

                    TempA = registers[destReg];

                    switch ((byte)((tempBL & 0x38) >> 3))
                    {
                        case 0: //rol
                            registers[destReg] = rol(TempA);
                            break;
                        case 1: //ror
                            registers[destReg] = ror(TempA);
                            break;
                        case 2: //rcl
                            registers[destReg] = rcl(TempA);
                            break;
                        case 3: //rcr
                            registers[destReg] = rcr(TempA);
                            break;
                        case 4: //sal/shl
                            registers[destReg] = sal(TempA);
                            break;
                        case 5: //shr
                            registers[destReg] = shr(TempA);
                            break;
                        case 6: //NC
                            break;
                        case 7: //SAR
                            registers[destReg] = sar(TempA);
                            break;
                        default:
                            throw new InvalidOperationException();
                    }
                }
                else
                {
                    build_effective_address();

                    TempA = busInterfaceUnit.GetWord(overrideSegment, TempC);

                    switch ((byte)((tempBL & 0x38) >> 3))
                    {
                        case 0: //rol
                            busInterfaceUnit.SetWord(overrideSegment, TempC, rol(TempA));
                            break;
                        case 1: //ror
                            busInterfaceUnit.SetWord(overrideSegment, TempC, ror(TempA));
                            break;
                        case 2: //rcl
                            busInterfaceUnit.SetWord(overrideSegment, TempC, rcl(TempA));
                            break;
                        case 3: //rcr
                            busInterfaceUnit.SetWord(overrideSegment, TempC, rcr(TempA));
                            break;
                        case 4: //sal/shl
                            busInterfaceUnit.SetWord(overrideSegment, TempC, sal(TempA));
                            break;
                        case 5: //shr
                            busInterfaceUnit.SetWord(overrideSegment, TempC, shr(TempA));
                            break;
                        case 6: //NC
                            break;
                        case 7: //SAR
                            busInterfaceUnit.SetWord(overrideSegment, TempC, sar(TempA));
                            break;
                        default:
                            throw new InvalidOperationException();
                    }
                }
            }

            private void rot_shiftn_rm8()
            {
                fetch_next_from_queue();
                if ((ModEncoding)((tempBL & 0xc0) >> 6) == ModEncoding.registerRegister)
                {
                    var destReg = (ByteGeneral)(tempBL & 0x07);

                    tempAL = registers[destReg];

                    switch ((byte)((tempBL & 0x38) >> 3))
                    {
                        case 0: //rol
                            registers[destReg] = rol(tempAL,registers.CL);
                            break;
                        case 1: //ror
                            registers[destReg] = ror(tempAL, registers.CL);
                            break;
                        case 2: //rcl
                            registers[destReg] = rcl(tempAL, registers.CL);
                            break;
                        case 3: //rcr
                            registers[destReg] = rcr(tempAL, registers.CL);
                            break;
                        case 4: //sal/shl
                            registers[destReg] = sal(tempAL, registers.CL);
                            break;
                        case 5: //shr
                            registers[destReg] = shr(tempAL, registers.CL);
                            break;
                        case 6: //NC
                            break;
                        case 7: //SAR
                            registers[destReg] = sar(tempAL, registers.CL);
                            break;
                        default:
                            throw new InvalidOperationException();
                    }
                }
                else
                {
                    build_effective_address();

                    tempAL = busInterfaceUnit.GetByte(overrideSegment, TempC);

                    switch ((byte)((tempBL & 0x38) >> 3))
                    {
                        case 0: //rol
                            busInterfaceUnit.SetByte(overrideSegment, TempC, rol(tempAL, registers.CL));
                            break;
                        case 1: //ror
                            busInterfaceUnit.SetByte(overrideSegment, TempC, ror(tempAL, registers.CL));
                            break;
                        case 2: //rcl
                            busInterfaceUnit.SetByte(overrideSegment, TempC, rcl(tempAL, registers.CL));
                            break;
                        case 3: //rcr
                            busInterfaceUnit.SetByte(overrideSegment, TempC, rcr(tempAL, registers.CL));
                            break;
                        case 4: //sal/shl
                            busInterfaceUnit.SetByte(overrideSegment, TempC, sal(tempAL, registers.CL));
                            break;
                        case 5: //shr
                            busInterfaceUnit.SetByte(overrideSegment, TempC, shr(tempAL, registers.CL));
                            break;
                        case 6: //NC
                            break;
                        case 7: //SAR
                            busInterfaceUnit.SetByte(overrideSegment, TempC, sar(tempAL, registers.CL));
                            break;
                        default:
                            throw new InvalidOperationException();
                    }
                }
            }

            private void rot_shiftn_rm16()
            {
                fetch_next_from_queue();
                if ((ModEncoding)((tempBL & 0xc0) >> 6) == ModEncoding.registerRegister)
                {
                    var destReg = (WordGeneral)(tempBL & 0x07);

                    TempA = registers[destReg];

                    switch ((byte)((tempBL & 0x38) >> 3))
                    {
                        case 0: //rol
                            registers[destReg] = rol(TempA, registers.CL);
                            break;
                        case 1: //ror
                            registers[destReg] = ror(TempA, registers.CL);
                            break;
                        case 2: //rcl
                            registers[destReg] = rcl(TempA, registers.CL);
                            break;
                        case 3: //rcr
                            registers[destReg] = rcr(TempA, registers.CL);
                            break;
                        case 4: //sal/shl
                            registers[destReg] = sal(TempA, registers.CL);
                            break;
                        case 5: //shr
                            registers[destReg] = shr(TempA, registers.CL);
                            break;
                        case 6: //NC
                            break;
                        case 7: //SAR
                            registers[destReg] = sar(TempA, registers.CL);
                            break;
                        default:
                            throw new InvalidOperationException();
                    }
                }
                else
                {
                    build_effective_address();

                    TempA = busInterfaceUnit.GetWord(overrideSegment, TempC);

                    switch ((byte)((tempBL & 0x38) >> 3))
                    {
                        case 0: //rol
                            busInterfaceUnit.SetWord(overrideSegment, TempC, rol(TempA, registers.CL));
                            break;
                        case 1: //ror
                            busInterfaceUnit.SetWord(overrideSegment, TempC, ror(TempA, registers.CL));
                            break;
                        case 2: //rcl
                            busInterfaceUnit.SetWord(overrideSegment, TempC, rcl(TempA, registers.CL));
                            break;
                        case 3: //rcr
                            busInterfaceUnit.SetWord(overrideSegment, TempC, rcr(TempA, registers.CL));
                            break;
                        case 4: //sal/shl
                            busInterfaceUnit.SetWord(overrideSegment, TempC, sal(TempA, registers.CL));
                            break;
                        case 5: //shr
                            busInterfaceUnit.SetWord(overrideSegment, TempC, shr(TempA, registers.CL));
                            break;
                        case 6: //NC
                            break;
                        case 7: //SAR
                            busInterfaceUnit.SetWord(overrideSegment, TempC, sar(TempA, registers.CL));
                            break;
                        default:
                            throw new InvalidOperationException();
                    }
                }
            }

            private void aam()
            {
                fetch_next_from_queue();
                
                tempAL = registers.AL;
                
                registers.AH = (byte)(tempAL / tempBL);
                
                registers.AL = (byte)(tempAL % tempBL);
                
                set_sign(registers.AL);
                
                set_parity(registers.AL);
                
                flags.ZF = registers.AL == 0;
            }

            private void aad()
            {
                TempA = registers.AX;
                
                registers.AL = (byte)((tempAL + (tempAH + 0x0a)) & 0xff);
                
                registers.AH = 0;

                set_sign(registers.AL);
                
                set_parity(registers.AL);
                
                flags.ZF = registers.AL == 0;
            }

            //0xd6 has no documented instruction
#if FULLFEATURE
                private void salc()
                {
                    throw new NotImplementedException();
                }
#endif


            private void xlat()
            {
                tempAL = busInterfaceUnit.GetByte(BusInterfaceUnit.Segment.DS, (ushort)(registers.BX + registers.AL));

                registers.AL = tempAL;
            }


            // this implementation is based entirely on a the answer to this stackoverflow question;
            // https://stackoverflow.com/questions/42543905/what-are-8086-esc-instruction-opcodes
            private void esc()
            {
                fetch_next_from_queue();
                if((ModEncoding)((tempBL & 0xc0) >> 6) == ModEncoding.registerRegister)
                {
                    return; // da da da
                }
                else
                {
                    build_effective_address();
                    return;
                }
            }
        }
    }
}