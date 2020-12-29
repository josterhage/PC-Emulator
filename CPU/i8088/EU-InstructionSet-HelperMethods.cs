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
            /// Assumes that TempB already contains the ModRM byte and that any memory values are still in the instruction queue. Does not modify tempBL
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

            private byte set_flags_and_sum(byte dest, byte src)
            {
                ushort sum = (ushort)(dest + src);

                flags.AF = (dest & 0x0f) + (src & 0x0f) > 0x0f;

                flags.CF = sum > 0xff;
                sum &= 0xff;

                flags.OF = ((dest < 0x80) || (src < 0x80)) && (sum >= 0x80);
                flags.ZF = sum == 0;
                set_sign((byte)sum);
                set_parity((byte)sum);
                return (byte)sum;
            }

            private ushort set_flags_and_sum(ushort dest, ushort src)
            {
                uint sum = (uint)(dest + src);

                flags.AF = (dest & 0x0f) + (src & 0x0f) > 0x0f;


                flags.CF = sum > 0xffff;
                sum &= 0xffff;

                flags.OF = ((dest < 0x8000) || (src < 0x8000)) && (sum >= 0x8000);
                flags.ZF = sum == 0;
                set_sign((ushort)sum);
                set_parity((ushort)sum);
                return (ushort)sum;
            }

            private byte set_flags_and_sum_carry(byte dest, byte src)
            {
                var carry = flags.CF ? 1 : 0;

                return (byte)(set_flags_and_sum(dest, src) + carry);
            }

            private ushort set_flags_and_sum_carry(ushort dest, ushort src)
            {
                var carry = flags.CF ? 1 : 0;
                return (ushort)(set_flags_and_sum(dest, src) + carry);
            }

            private byte set_flags_and_or(byte dest, byte src)
            {
                byte result = (byte)(dest | src);
                flags.OF = false;
                flags.CF = false;
                flags.ZF = result == 0;
                set_sign(result);
                set_parity(result);

                return result;
            }

            private ushort set_flags_and_or(ushort dest, ushort src)
            {
                ushort result = (ushort)(dest | src);
                flags.OF = false;
                flags.CF = false;
                flags.ZF = result == 0;
                set_sign(result);
                set_parity(result);

                return result;
            }

            private byte set_flags_and_diff(byte dest, byte src)
            {
                byte diff = (byte)(dest - src);

                flags.AF = (dest & 0x0f) > (src & 0x0f);

                flags.CF = dest > src;

                flags.OF = (dest - src) < -128;

                flags.ZF = diff == 0;
                set_sign(diff);
                set_parity(diff);

                return diff;
            }

            private ushort set_flags_and_diff(ushort dest, ushort src)
            {
                ushort diff = (ushort)(dest - src);

                flags.AF = (dest & 0x0f) > (src & 0x0f);

                flags.CF = dest > src;

                flags.OF = (dest - src) < -32768;

                flags.ZF = diff == 0;

                set_sign(diff);
                set_parity(diff);

                return diff;
            }

            private byte set_flags_and_diff_borrow(byte dest, byte src)
            {
                var carry = flags.CF ? -1 : 0;

                return (byte)(set_flags_and_diff(dest, src) + carry);
            }

            private ushort set_flags_and_diff_borrow(ushort dest, ushort src)
            {
                var carry = flags.CF ? -1 : 0;

                return (ushort)(set_flags_and_diff(dest, src) + carry);
            }

            private byte set_flags_and_and(byte dest, byte src)
            {
                byte result = (byte)(dest & src);
                flags.OF = false;
                flags.CF = false;
                flags.ZF = result == 0;
                set_sign(result);
                set_parity(result);

                return result;
            }

            private ushort set_flags_and_and(ushort dest, ushort src)
            {
                ushort result = (ushort)(dest & src);
                flags.OF = false;
                flags.CF = false;
                flags.ZF = result == 0;
                set_sign(result);
                set_parity(result);

                return result;
            }

            private byte set_flags_and_xor(byte dest, byte src)
            {
                byte result = (byte)(dest ^ src);
                flags.OF = false;
                flags.CF = false;
                flags.ZF = result == 0;
                set_sign(result);
                set_parity(result);

                return result;
            }

            private ushort set_flags_and_xor(ushort dest, ushort src)
            {
                ushort result = (ushort)(dest ^ src);
                flags.OF = false;
                flags.CF = false;
                flags.ZF = result == 0;
                set_sign(result);
                set_parity(result);

                return result;
            }

            private ushort set_flags_and_imul(byte dest, byte src)
            {
                short result = 0;
                result = (short)(dest * src);
                ushort signtest = (ushort)(result & 0xff);
                signtest |= (ushort)(((signtest & 0x80) != 0) ? 0xff00 : 0);
                flags.CF = flags.OF = result == signtest;
                return (ushort)result;
            }

            private byte rol(byte value, byte count = 1)
            {
                bool sign = (value & 0x80) != 0;

                byte mask = (byte)((0xff - ((1 << 8 - count) - 1)) & value);

                mask >>= 8 - count;

                value <<= count;

                value |= mask;

                flags.CF = (value & 0x01) != 0;

                flags.OF = (count == 1) && (sign != ((value & 0x80) != 0));

                return value;
            }

            private byte ror(byte value, byte count = 1)
            {
                bool sign = (value & 0x80) != 0;

                byte mask = (byte)(((1 << count) - 1) & value);

                mask <<= 8 - count;

                value >>= count;

                value |= mask;

                flags.CF = (value & 0x80) != 0;

                flags.OF = (count == 1) && (sign != ((value & 0x80) != 0));

                return value;
            }

            private byte rcl(byte value, byte count = 1)
            {
                bool sign = (value & 0x80) != 0;

                byte mask = (byte)((0xff - ((1 << 8 - count) - 1)) & value);

                mask >>= 1;

                mask |= flags.CF ? 0x80 : 0;

                mask >>= 8 - count - 1;

                flags.CF = (mask & 0x01) != 0;

                mask >>= 1;

                value <<= count;

                value |= mask;

                flags.OF = (count == 1) && (sign != ((value & 0x80) != 0));

                return value;
            }

            private byte rcr(byte value, byte count = 1)
            {
                bool sign = (value & 0x80) != 0;

                byte mask = (byte)(((1 << count) - 1) & value);

                mask <<= 1;

                mask |= flags.CF ? 0x01 : 0;

                mask <<= 8 - count - 1;

                flags.CF = (mask & 0x80) != 0;

                mask <<= 1;

                value >>= count;

                value |= mask;

                flags.OF = (count == 1) && (sign != ((value & 0x80) != 0));

                return value;
            }

            private byte sal(byte value, byte count = 1)
            {
                bool sign = (value & 0x80) != 0;

                flags.CF = (value & (1 << 8 - count)) != 0;

                value <<= count;

                flags.SF = (value & 0x80) != 0;

                flags.OF = sign != flags.SF;

                flags.ZF = value == 0;

                set_parity(value);

                return value;
            }

            private byte shr(byte value, byte count = 1)
            {
                bool sign = (value & 0x80) != 0;

                flags.SF = false;

                flags.CF = (value & (1 << count - 1)) != 0;

                value >>= count;

                flags.OF = sign;

                flags.ZF = value == 0;

                set_parity(value);

                return value;
            }

            private byte sar(byte value, byte count = 1)
            {
                flags.SF = (value & 0x80) != 0;

                flags.CF = (value & (1 << count -1)) != 0;

                value >>= count;

                byte mask = (byte)(0xff - ((1 << 8 - count) - 1));

                value |= flags.SF ? mask : 0;

                flags.OF = false;

                flags.ZF = value == 0;

                set_parity(value);

                return value;
            }

            private ushort rol(ushort value, byte count = 1)
            {
                bool sign = (value & 0x8000) != 0;

                ushort mask = (ushort)((0xffff - ((1 << 16 - count) - 1)) & value);

                mask >>= 16 - count;

                value <<= count;

                value |= mask;

                flags.CF = (value & 0x01) != 0;

                flags.OF = (count == 1) && (sign != ((value & 0x8000) != 0));

                return value;
            }

            private ushort ror(ushort value, byte count = 1)
            {
                bool sign = (value & 0x8000) != 0;

                ushort mask = (ushort)(((1 << count) - 1) & value);

                mask <<= 16 - count;

                value >>= count;

                value |= mask;

                flags.CF = (value & 0x8000) != 0;

                flags.OF = (count == 1) && (sign != ((value & 0x8000) != 0));

                return value;
            }

            private ushort rcl(ushort value, byte count = 1)
            {
                bool sign = (value & 0x8000) != 0;

                ushort mask = (ushort)((0xffff - ((1 << 16 - count) - 1)) & value);

                mask >>= 1;

                mask |= flags.CF ? 0x8000 : 0;

                mask >>= 16 - count - 1;

                flags.CF = (mask & 0x01) != 0;

                mask >>= 1;

                value <<= count;

                value |= mask;

                flags.OF = (count == 1) && (sign != ((value & 0x8000) != 0));

                return value;
            }

            private ushort rcr(ushort value, byte count = 1)
            {
                bool sign = (value & 0x8000) != 0;

                ushort mask = (ushort)(((1 << count) - 1) & value);

                mask <<= 1;

                mask |= flags.CF ? 0x01 : 0;

                mask <<= 16 - count - 1;

                flags.CF = (mask & 0x8000) != 0;

                mask <<= 1;

                value >>= count;

                value |= mask;

                flags.OF = (count == 1) && (sign != ((value & 0x8000) != 0));

                return value;
            }

            private ushort sal(ushort value, byte count = 1)
            {
                bool sign = (value & 0x8000) != 0;

                flags.CF = (value & (1 << 16 - count)) != 0;

                value <<= count;

                flags.SF = (value & 0x8000) != 0;

                flags.OF = sign != flags.SF;

                flags.ZF = value == 0;

                set_parity(value);

                return value;
            }

            private ushort shr(ushort value, byte count = 1)
            {
                bool sign = (value & 0x8000) != 0;

                flags.SF = false;

                flags.CF = (value & (1 << count - 1)) != 0;

                value >>= count;

                flags.OF = sign;

                flags.ZF = value == 0;

                set_parity(value);

                return value;
            }

            private ushort sar(ushort value, byte count = 1)
            {
                flags.SF = (value & 0x8000) != 0;

                flags.CF = (value & (1 << count - 1)) != 0;

                value >>= count;

                ushort mask = (byte)(0xffff - ((1 << 16 - count) - 1));

                value |= flags.SF ? mask : 0;

                flags.OF = false;

                flags.ZF = value == 0;

                set_parity(value);

                return value;
            }
        }
    }
}
