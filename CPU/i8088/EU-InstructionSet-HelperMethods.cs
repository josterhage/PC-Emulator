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
                RmEncoding rm = (RmEncoding)((tempBL & 0x38) >> 3);

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
                busInterfaceUnit.SetByte(tempBL, TempC, tempAL);
            }

            /// <summary>
            /// Asks the BIU to write the byte in tempA to the memory address pointed at by tempC
            /// tempBL contains segment override information if applicable
            /// </summary>
            private void word_to_memory()
            {
                busInterfaceUnit.SetWord(tempBL, TempC, TempA);
            }

            /// <summary>
            /// Asks the BIU to retrieve a byte from the memory address pointed at by tempC
            /// byte is placed in TempBL
            /// </summary>
            private void byte_from_memory()
            {
                tempAL = busInterfaceUnit.GetByte(tempBL, TempC);
            }

            /// <summary>
            /// Asks the BIU to retrieve a word from the memory address pointed at by tempC
            /// word is placed in TempB
            /// </summary>
            private void word_from_memory()
            {
                TempA = busInterfaceUnit.GetByte(tempBL, TempC);
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
        }
    }
}
