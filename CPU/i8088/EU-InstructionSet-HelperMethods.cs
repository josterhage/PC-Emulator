using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPU.i8088
{
    public partial class Processor
    {
        private partial class ExecutionUnit
        {
            /// <summary>
            /// Builds the effective address for the current operation based on mod/rm and any relevant immediate values and stores it in TempC
            /// </summary>
            /// <remarks>
            /// Assumes that TempB already contains the ModRM byte and that any memory values are still in the instruction queue
            /// </remarks>
            private void build_effective_address()
            {
                ModEncoding mod = (ModEncoding)((tempBL & 0xC0) >> 6);
                RmEncoding rm = (RmEncoding)(tempBL & 0x07);

                switch (mod)
                {
                    case ModEncoding.registerDisplacement:
                        if (rm == RmEncoding.BP)
                        {
                            fetch_next_from_queue();
                            tempCL = tempBL;
                            fetch_next_from_queue();
                            tempCH = tempBL;
                        }
                        else
                        {
                            TempC = 0;
                        }
                        break;
                    case ModEncoding.byteDisplacement:
                        fetch_next_from_queue();
                        tempCL = tempBL;
                        break;
                    case ModEncoding.wordDisplacement:
                        fetch_next_from_queue();
                        tempCL = tempBL;
                        fetch_next_from_queue();
                        tempCH = tempBL;
                        break;
                    case ModEncoding.registerRegister:
                        TempC = 0;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                switch (rm)
                {
                    case RmEncoding.BXSI:
                        TempC += registers.BX;
                        TempC += registers.SI;
                        break;
                    case RmEncoding.BXDI:
                        TempC += registers.BX;
                        TempC += registers.DI;
                        break;
                    case RmEncoding.BPSI:
                        TempC += registers.BP;
                        TempC += registers.SI;
                        break;
                    case RmEncoding.BPDI:
                        TempC += registers.BP;
                        TempC += registers.DI;
                        break;
                    case RmEncoding.SI:
                        TempC += registers.SI;
                        break;
                    case RmEncoding.DI:
                        TempC += registers.DI;
                        break;
                    case RmEncoding.BP:
                        if (mod != ModEncoding.registerDisplacement)
                        {
                            TempC += registers.BP;
                        }
                        break;
                    case RmEncoding.BX:
                        TempC += registers.BX;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            /// <summary>
            /// Retrieves the next byte from the instruction queue and places it in tempBL
            /// </summary>
            /// <remarks>
            /// Blocks if no byte is available
            /// </remarks>
            private void fetch_next_from_queue()
            {
                tempBL = busInterfaceUnit.GetNextFromQueue();
            }

            /// <summary>
            /// Asks the BIU to write the byte in tempAL to the memory address pointed at by tempC
            /// tempBL contains segment override information if applicable
            /// </summary>
            private void byte_to_memory()
            {
                busInterfaceUnit.SetByte(overrideSegment, TempC, tempAL);
            }

            /// <summary>
            /// Asks the BIU to write the byte in tempA to the memory address pointed at by tempC
            /// tempBL contains segment override information if applicable
            /// </summary>
            private void word_to_memory()
            {
                //busInterfaceUnit.SetWord(overrideSegment, TempC, TempA);
            }

            /// <summary>
            /// Asks the BIU to retrieve a byte from the memory address pointed at by tempC
            /// byte is placed in TempBL
            /// </summary>
            private void byte_from_memory()
            {
                tempAL = busInterfaceUnit.GetByte(overrideSegment, TempC);
            }

            /// <summary>
            /// Asks the BIU to retrieve a word from the memory address pointed at by tempC
            /// word is placed in TempB
            /// </summary>
            private void word_from_memory()
            {
                TempA = busInterfaceUnit.GetByte(overrideSegment, TempC);
            }

            /// <summary>
            /// Tells the BIU to increment the IP by tempCL
            /// </summary>
            private void jump_short()
            {
                throw new NotImplementedException();
            }

            /// <summary>
            /// Tells the BIU to change the IP to tempC
            /// </summary>
            private void jump_near()
            {
                throw new NotImplementedException();
            }

            /// <summary>
            /// Tells the BIU to change the CS to TempB and the IP to TempC
            /// </summary>
            private void jump_far()
            {
                throw new NotImplementedException();
            }

            private void set_sign(byte value)
            {
                flags.SF = (value & 128) != 0;
            }

            private void set_sign(ushort value)
            {
                flags.SF = (value & 32768) != 0;
            }

            private void set_parity(byte value)
            {
                flags.PF = true;
                for (int i = 0; i < 8; i++)
                {
                    flags.PF ^= (value & (1 << i)) != 0;
                }
            }

            private void set_parity(ushort value)
            {
                flags.PF = true;
                for (int i = 0; i < 16; i++)
                {
                    flags.PF ^= (value & (1 << i)) != 0;
                }
            }

            private byte set_flags_and_sum(byte left, byte right)
            {
                ushort sum = (ushort)(left + right);

                flags.AF = (left & 0x0f) + (right & 0x0f) > 0x0f;

                flags.CF = sum > 0xff;
                sum &= 0xff;

                flags.OF = ((left < 0x80) || (right < 0x80)) && (sum >= 0x80);
                flags.ZF = sum == 0;
                set_sign((byte)sum);
                set_parity((byte)sum);
                return (byte)sum;
            }

            private ushort set_flags_and_sum(ushort left, ushort right)
            {
                uint sum = (uint)(left + right);

                flags.AF = (left & 0x0f) + (right & 0x0f) > 0x0f;


                flags.CF = sum > 0xffff;
                sum &= 0xffff;

                flags.OF = ((left < 0x8000) || (right < 0x8000)) && (sum >= 0x8000);
                flags.ZF = sum == 0;
                set_sign((ushort)sum);
                set_parity((ushort)sum);
                return (ushort)sum;
            }

            private byte set_flags_and_sum_carry(byte left, byte right)
            {
                var carry = flags.CF ? 1 : 0;

                return (byte)(set_flags_and_sum(left, right) + carry);
            }

            private ushort set_flags_and_sum_carry(ushort left, ushort right)
            {
                var carry = flags.CF ? 1 : 0;
                return (ushort)(set_flags_and_sum(left, right) + carry);
            }

            private byte set_flags_and_or(byte left, byte right)
            {
                byte result = (byte)(left | right);
                flags.OF = false;
                flags.CF = false;
                flags.ZF = result == 0;
                set_sign(result);
                set_parity(result);

                return result;
            }

            private ushort set_flags_and_or(ushort left, ushort right)
            {
                ushort result = (ushort)(left | right);
                flags.OF = false;
                flags.CF = false;
                flags.ZF = result == 0;
                set_sign(result);
                set_parity(result);

                return result;
            }

            private byte set_flags_and_diff(byte left, byte right)
            {
                byte diff = (byte)(left - right);

                flags.AF = (left & 0x0f) > (right & 0x0f);

                flags.CF = left > right;

                flags.OF = (left - right) < -128;

                flags.ZF = diff == 0;
                set_sign(diff);
                set_parity(diff);

                return diff;
            }

            private ushort set_flags_and_diff(ushort left, ushort right)
            {
                ushort diff = (ushort)(left - right);

                flags.AF = (left & 0x0f) > (right & 0x0f);

                flags.CF = left > right;

                flags.OF = (left - right) < -32768;

                flags.ZF = diff == 0;

                set_sign(diff);
                set_parity(diff);

                return diff;
            }

            private byte set_flags_and_diff_borrow(byte left, byte right)
            {
                var carry = flags.CF ? -1 : 0;

                return (byte)(set_flags_and_diff(left, right) + carry);
            }

            private ushort set_flags_and_diff_borrow(ushort left, ushort right)
            {
                var carry = flags.CF ? -1 : 0;

                return (ushort)(set_flags_and_diff(left, right) + carry);
            }

            private byte set_flags_and_and(byte left, byte right)
            {
                byte result = (byte)(left & right);
                flags.OF = false;
                flags.CF = false;
                flags.ZF = result == 0;
                set_sign(result);
                set_parity(result);

                return result;
            }

            private ushort set_flags_and_and(ushort left, ushort right)
            {
                ushort result = (ushort)(left & right);
                flags.OF = false;
                flags.CF = false;
                flags.ZF = result == 0;
                set_sign(result);
                set_parity(result);

                return result;
            }
        }
    }
}
