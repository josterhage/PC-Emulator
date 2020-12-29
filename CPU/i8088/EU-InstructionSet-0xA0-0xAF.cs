using System;

namespace CPU.i8088
{
    public partial class Processor
    {
        private partial class ExecutionUnit
        {
            private void mov_al_m8()
            {
                fetch_next_from_queue();
                tempAL = tempBL;
                fetch_next_from_queue();
                tempAH = tempBL;

                registers.AL = busInterfaceUnit.GetByte(overrideSegment, TempA);
            }

            private void mov_ax_m16()
            {
                fetch_next_from_queue();
                tempAL = tempBL;
                fetch_next_from_queue();
                tempAH = tempBL;

                registers.AX = busInterfaceUnit.GetWord(overrideSegment, TempA);
            }

            private void mov_m8_al()
            {
                fetch_next_from_queue();
                tempAL = tempBL;
                fetch_next_from_queue();
                tempAH = tempBL;

                busInterfaceUnit.SetByte(overrideSegment, TempA, registers.AL);
            }

            private void mov_m16_ax()
            {
                fetch_next_from_queue();
                tempAL = tempBL;
                fetch_next_from_queue();
                tempAH = tempBL;

                busInterfaceUnit.SetWord(overrideSegment, TempA, registers.AX);
            }

            private void movsb()
            {
                tempAL = busInterfaceUnit.GetByte(overrideSegment, registers.SI);
                registers.SI = flags.DF ? (ushort)(registers.SI - 1) : (ushort)(registers.SI + 1);
                busInterfaceUnit.SetByte(BusInterfaceUnit.Segment.ES, registers.DI, tempAL);
                registers.DI = flags.DF ? (ushort)(registers.DI - 1) : (ushort)(registers.DI + 1);
            }

            private void movsw()
            {
                TempA = busInterfaceUnit.GetWord(overrideSegment, registers.SI);
                registers.SI = flags.DF ? (ushort)(registers.SI - 2) : (ushort)(registers.SI + 2);
                busInterfaceUnit.SetWord(BusInterfaceUnit.Segment.ES, registers.DI, TempA);
                registers.DI = flags.DF ? (ushort)(registers.DI - 2) : (ushort)(registers.DI + 2);
            }

            private void cmpsb()
            {
                tempAL = busInterfaceUnit.GetByte(overrideSegment, registers.SI);
                registers.SI = flags.DF ? (ushort)(registers.SI - 1) : (ushort)(registers.SI + 1);
                tempBL = busInterfaceUnit.GetByte(BusInterfaceUnit.Segment.ES, registers.DI);
                registers.DI = flags.DF ? (ushort)(registers.DI - 1) : (ushort)(registers.DI + 1); 
                tempCL = set_flags_and_diff(tempAL, tempBL);
            }

            private void cmpsw()
            {
                TempA = busInterfaceUnit.GetWord(overrideSegment, registers.SI);
                registers.SI = flags.DF ? (ushort)(registers.SI - 2) : (ushort)(registers.SI + 2);
                TempB = busInterfaceUnit.GetWord(BusInterfaceUnit.Segment.ES, registers.DI);
                registers.DI = flags.DF ? (ushort)(registers.DI - 2) : (ushort)(registers.DI + 2);
                TempC = set_flags_and_diff(TempA, TempB);
            }

            private void test_al_i8()
            {
                fetch_next_from_queue();
                tempAL = set_flags_and_and(registers.AL, tempBL);
            }

            private void test_ax_i16()
            {
                fetch_next_from_queue();
                tempAL = tempBL;
                fetch_next_from_queue();
                tempAH = tempBL;
                TempB = set_flags_and_and(registers.AH, TempA);
            }

            private void stosb()
            {
                busInterfaceUnit.SetByte(BusInterfaceUnit.Segment.ES, registers.DI, registers.AL);
                registers.DI = flags.DF ? (ushort)(registers.DI - 1) : (ushort)(registers.DI + 1);
            }

            private void stosw()
            {
                busInterfaceUnit.SetWord(BusInterfaceUnit.Segment.ES, registers.DI, registers.AX);
                registers.DI = flags.DF ? (ushort)(registers.DI - 2) : (ushort)(registers.DI + 2);
            }

            private void lodsb()
            {
                registers.AL = busInterfaceUnit.GetByte(overrideSegment, registers.SI);
                registers.SI = flags.DF ? (ushort)(registers.SI - 1) : (ushort)(registers.SI + 1);
            }

            private void lodsw()
            {
                registers.AX = busInterfaceUnit.GetWord(overrideSegment, registers.SI);
                registers.SI = flags.DF ? (ushort)(registers.SI - 2) : (ushort)(registers.SI + 2);
            }

            private void scasb()
            {
                tempAL = busInterfaceUnit.GetByte(BusInterfaceUnit.Segment.ES, registers.DI);
                registers.DI = flags.DF ? (ushort)(registers.DI - 1) : (ushort)(registers.DI + 1);
                tempBL = set_flags_and_diff(registers.AL, tempAL);
            }

            private void scasw()
            {
                TempA = busInterfaceUnit.GetWord(BusInterfaceUnit.Segment.ES, registers.DI);
                registers.DI = flags.DF ? (ushort)(registers.DI - 2) : (ushort)(registers.DI + 2);
                TempB = set_flags_and_diff(registers.AX, TempA);
            }
        }
    }
}

