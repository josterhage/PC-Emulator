using System;

namespace CPU.i8088
{
    public partial class Processor
    {
        private partial class ExecutionUnit
        {
            private void mov_al_i8()
            {
                fetch_next_from_queue();
                registers.AL = tempBL;
            }

            private void mov_cl_i8()
            {
                throw new NotImplementedException();
            }

            private void mov_dl_i8()
            {
                throw new NotImplementedException();
            }

            private void mov_bl_i8()
            {
                throw new NotImplementedException();
            }

            private void mov_ah_i8()
            {
                throw new NotImplementedException();
            }

            private void mov_ch_i8()
            {
                throw new NotImplementedException();
            }

            private void mov_dh_i8()
            {
                throw new NotImplementedException();
            }

            private void mov_bh_i8()
            {
                throw new NotImplementedException();
            }

            private void mov_ax_i16()
            {
                throw new NotImplementedException();
            }

            private void mov_cx_i16()
            {
                throw new NotImplementedException();
            }

            private void mov_dx_i16()
            {
                throw new NotImplementedException();
            }

            private void mov_bx_i16()
            {
                throw new NotImplementedException();
            }

            private void mov_sp_i16()
            {
                throw new NotImplementedException();
            }

            private void mov_bp_i16()
            {
                throw new NotImplementedException();
            }

            private void mov_si_i16()
            {
                throw new NotImplementedException();
            }

            private void mov_di_i16()
            {
                throw new NotImplementedException();
            }
        }
    }
}

