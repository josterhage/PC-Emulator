using System;

namespace CPU.i8088
{
    public partial class Processor
    {
        private partial class ExecutionUnit
        {
            private void jo()
            {
                fetch_next_from_queue();
                if (flags.OF)
                {
                    busInterfaceUnit.JumpShort(tempBL);
                }
            }

            private void jno()
            {
                fetch_next_from_queue();
                if (!flags.OF)
                {
                    busInterfaceUnit.JumpShort(tempBL);
                }
            }

            private void jb_jnae_jc()
            {
                fetch_next_from_queue();
                if(flags.CF)
                {
                    busInterfaceUnit.JumpShort(tempBL);
                }
            }

            private void jnb_jae_jnc()
            {
                fetch_next_from_queue();
                if (!flags.CF)
                {
                    busInterfaceUnit.JumpShort(tempBL);
                }
            }

            private void je_jz()
            {
                fetch_next_from_queue();
                if (flags.ZF)
                {
                    busInterfaceUnit.JumpShort(tempBL);
                }
            }

            private void jne_jnz()
            {
                fetch_next_from_queue();
                if (!flags.ZF)
                {
                    busInterfaceUnit.JumpShort(tempBL);
                }
            }

            private void jbe_jna()
            {
                fetch_next_from_queue();
                if(flags.CF || flags.ZF)
                {
                    busInterfaceUnit.JumpShort(tempBL);
                }
            }

            private void jnbe_ja()
            {
                fetch_next_from_queue();
                if (!flags.CF || !flags.ZF)
                {
                    busInterfaceUnit.JumpShort(tempBL);
                }
            }

            private void js()
            {
                fetch_next_from_queue();
                if (flags.SF)
                {
                    busInterfaceUnit.JumpShort(tempBL);
                }
            }

            private void jns()
            {
                fetch_next_from_queue();
                if (!flags.SF)
                {
                    busInterfaceUnit.JumpShort(tempBL);
                }
            }

            private void jp_jpe()
            {

                fetch_next_from_queue();
                if (flags.PF)
                {
                    busInterfaceUnit.JumpShort(tempBL);
                }
            }

            private void jnp_jpo()
            {
                fetch_next_from_queue();
                if (!flags.PF)
                {
                    busInterfaceUnit.JumpShort(tempBL);
                }
            }

            private void jl_jnge()
            {

                fetch_next_from_queue();
                if (flags.SF ^ flags.OF)
                {
                    busInterfaceUnit.JumpShort(tempBL);
                }
            }

            private void jnl_jge()
            {

                fetch_next_from_queue();
                if (!(flags.SF ^ flags.OF))
                {
                    busInterfaceUnit.JumpShort(tempBL);
                }
            }

            private void jle_jng()
            {
                fetch_next_from_queue();
                if ((flags.SF ^ flags.OF) || flags.ZF)
                {
                    busInterfaceUnit.JumpShort(tempBL);
                }
            }

            private void jnle_jg()
            {
                fetch_next_from_queue();
                if (!(flags.SF ^ flags.OF) || !flags.ZF)
                {
                    busInterfaceUnit.JumpShort(tempBL);
                }
            }
        }
    }
}

