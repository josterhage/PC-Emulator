using System;

namespace CPU.i8088
{
    public partial class Processor
    {
        private partial class ExecutionUnit
        {
            private void rot_shift1_rm8()
            {
                throw new NotImplementedException();
            }

            private void rot_shift1_rm16()
            {
                throw new NotImplementedException();
            }

            private void rot_shiftn_rm8()
            {
                throw new NotImplementedException();
            }

            private void rot_shiftn_rm16()
            {
                throw new NotImplementedException();
            }

            private void aam()
            {
                throw new NotImplementedException();
            }

            private void aad()
            {
                throw new NotImplementedException();
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
                throw new NotImplementedException();
            }

            private void esc_d8()
            {
                throw new NotImplementedException();
            }

            private void esc_d9()
            {
                throw new NotImplementedException();
            }

            private void esc_da()
            {
                throw new NotImplementedException();
            }

            private void esc_db()
            {
                throw new NotImplementedException();
            }

            private void esc_dc()
            {
                throw new NotImplementedException();
            }

            private void esc_dd()
            {
                throw new NotImplementedException();
            }

            private void esc_de()
            {
                throw new NotImplementedException();
            }

            private void esc_df()
            {
                throw new NotImplementedException();
            }
        }
    }
}