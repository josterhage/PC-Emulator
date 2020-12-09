using System;

namespace CPU.i8088
{
    public partial class Processor
    {
        private partial class ExecutionUnit
        {
            //0xc0 and 0xc1 have no instructions

            private void ret_near_i16()
            {
                throw new NotImplementedException();
            }

            private void ret_near()
            {
                throw new NotImplementedException();
            }

            private void lds_rm16()
            {
                throw new NotImplementedException();
            }

            private void les_rm16()
            {
                throw new NotImplementedException();
            }

            private void mov_m8_i8()
            {
                throw new NotImplementedException();
            }

            private void mov_m16_i16()
            {
                throw new NotImplementedException();
            }

            //0xc8 and 0xc9 have no instructions

            private void ret_far_i16()
            {
                throw new NotImplementedException();
            }

            private void ret_far()
            {
                throw new NotImplementedException();
            }

            private void int_3()
            {
                throw new NotImplementedException();
            }

            private void int_i8()
            {
                throw new NotImplementedException();
            }

            private void int_o()
            {
                throw new NotImplementedException();
            }

            private void iret()
            {
                throw new NotImplementedException();
            }
        }
    }
}
