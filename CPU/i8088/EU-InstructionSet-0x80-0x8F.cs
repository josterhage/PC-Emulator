using System;

namespace CPU.i8088
{
    public partial class Processor
    {
        private partial class ExecutionUnit
        {
            private void o80h_rm8_i8()
            {
                throw new NotImplementedException();
            }

            private void o81h_rm16_i16()
            {
                throw new NotImplementedException();
            }

            private void o82h_rm8_i16()
            {
                throw new NotImplementedException();
            }

            private void o83h_rm16_i16()
            {
                throw new NotImplementedException();
            }

            private void test_rm8_r8()
            {
                throw new NotImplementedException();
            }

            private void test_rm16_r16()
            {
                throw new NotImplementedException();
            }

            private void xchg_rm8_r8()
            {
                throw new NotImplementedException();
            }

            private void xchg_rm16_r16()
            {
                throw new NotImplementedException();
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
                    build_effective_address();
                    var srcReg = (ByteGeneral)((tempBL & 0x38) >> 3);

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
                    build_effective_address();
                    var srcReg = (WordGeneral)((tempBL & 0x38) >> 3);

                    busInterfaceUnit.SetWord(overrideSegment, TempC, registers[srcReg]);
                }
            }

            private void mov_r8_rm8()
            {
                //throw new NotImplementedException();
            }

            private void mov_r16_rm16()
            {
                throw new NotImplementedException();
            }

            private void mov_rm16_seg()
            {
                throw new NotImplementedException();
            }

            private void lea_r16_m16()
            {
                throw new NotImplementedException();
            }

            private void mov_seg_rm16()
            {
                throw new NotImplementedException();
            }

            private void pop_rm16()
            {
                throw new NotImplementedException();
            }
        }
    }
}
