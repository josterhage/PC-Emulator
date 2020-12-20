using System;

namespace CPU.i8088
{
    public partial class Processor
    {
        private partial class ExecutionUnit
        {
            private void inc_ax()
            {
                //AF, OF, PF, SF, ZF;
                registers.AX++;
                flags.AF = registers.AX == 16;
                flags.OF = registers.AX == 0x8000;
                set_parity(registers.AX);
                set_sign(registers.AX);
                flags.ZF = registers.AX == 0;
            }

            private void inc_cx()
            {
                registers.CX++;
                flags.AF = registers.CX == 16;
                flags.OF = registers.CX == 0x8000;
                set_parity(registers.CX);
                set_sign(registers.CX);
                flags.ZF = registers.CX == 0;
            }

            private void inc_dx()
            {
                registers.DX++;
                flags.AF = registers.DX == 16;
                flags.OF = registers.DX == 0x8000;
                set_parity(registers.DX);
                set_sign(registers.DX);
                flags.ZF = registers.DX == 0;
            }

            private void inc_bx()
            {
                registers.BX++;
                flags.AF = registers.BX == 16;
                flags.OF = registers.BX == 0x8000;
                set_parity(registers.BX);
                set_sign(registers.BX);
                flags.ZF = registers.BX == 0;
            }

            private void inc_sp()
            {
                registers.SP++;
                flags.AF = registers.SP == 16;
                flags.OF = registers.SP == 0x8000;
                set_parity(registers.SP);
                set_sign(registers.SP);
                flags.ZF = registers.SP == 0;
            }

            private void inc_bp()
            {
                registers.BP++;
                flags.AF = registers.BP == 16;
                flags.OF = registers.BP == 0x8000;
                set_parity(registers.BP);
                set_sign(registers.BP);
                flags.ZF = registers.BP == 0;
            }

            private void inc_si()
            {
                registers.SI++;
                flags.AF = registers.SI == 16;
                flags.OF = registers.SI == 0x8000;
                set_parity(registers.SI);
                set_sign(registers.SI);
                flags.ZF = registers.SI == 0;
            }

            private void inc_di()
            {
                registers.DI++;
                flags.AF = registers.DI == 16;
                flags.OF = registers.DI == 0x8000;
                set_parity(registers.DI);
                set_sign(registers.DI);
                flags.ZF = registers.DI == 0;
            }

            private void dec_ax()
            {
                registers.AX--;
                flags.AF = registers.AX == 16;
                flags.OF = registers.AX == 0x8000;
                set_parity(registers.AX);
                set_sign(registers.AX);
                flags.ZF = registers.AX == 0;
            }

            private void dec_cx()
            {
                registers.CX--;
                flags.AF = registers.CX == 16;
                flags.OF = registers.CX == 0x8000;
                set_parity(registers.CX);
                set_sign(registers.CX);
                flags.ZF = registers.CX == 0;
            }

            private void dec_dx()
            {
                registers.DX--;
                flags.AF = registers.DX == 16;
                flags.OF = registers.DX == 0x8000;
                set_parity(registers.DX);
                set_sign(registers.DX);
                flags.ZF = registers.DX == 0;
            }

            private void dec_bx()
            {
                registers.BX--;
                flags.AF = registers.BX == 16;
                flags.OF = registers.BX == 0x8000;
                set_parity(registers.BX);
                set_sign(registers.BX);
                flags.ZF = registers.BX == 0;
            }

            private void dec_sp()
            {
                registers.SP--;
                flags.AF = registers.SP == 16;
                flags.OF = registers.SP == 0x8000;
                set_parity(registers.SP);
                set_sign(registers.SP);
                flags.ZF = registers.SP == 0;
            }

            private void dec_bp()
            {
                registers.BP--;
                flags.AF = registers.BP == 16;
                flags.OF = registers.BP == 0x8000;
                set_parity(registers.BP);
                set_sign(registers.BP);
                flags.ZF = registers.BP == 0;
            }

            private void dec_si()
            {
                registers.SI--;
                flags.AF = registers.SI == 16;
                flags.OF = registers.SI == 0x8000;
                set_parity(registers.SI);
                set_sign(registers.SI);
                flags.ZF = registers.SI == 0;
            }

            private void dec_di()
            {
                registers.DI--;
                flags.AF = registers.DI == 16;
                flags.OF = registers.DI == 0x8000;
                set_parity(registers.DI);
                set_sign(registers.DI);
                flags.ZF = registers.DI == 0;
            }
        }
    }
}

