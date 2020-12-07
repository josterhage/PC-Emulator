using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPU
{
    public partial class ExecutionUnit
    {
        private void execute()
        {
            instructionSet[opcode]?.Invoke();
            tick += fetch_opcode;
        }

        private void fetch_opcode(object sender, EventArgs e)
        {
            tick -= fetch_opcode;
            while (!busInterfaceUnit.QueueReady) ;
            Opcode = (byte)busInterfaceUnit.GetNextInstructionByte();
            //TODO: Check for prefixes (segment override, lock, repeat)
            if (needModRm[opcode])
            {
                fetch_mod_rm();
            }
            else
            {
                execute();
            }
        }

        private void fetch_mod_rm()
        {
            while (!busInterfaceUnit.QueueReady) ;
            ModRM = (byte)busInterfaceUnit.GetNextInstructionByte();
            execute();
        }

        private void fetch_data_byte()
        {
            while (!busInterfaceUnit.QueueReady) ;
            dataByteLow = (byte)busInterfaceUnit.GetNextInstructionByte();
        }

        private void fetch_data_word()
        {
            while (!busInterfaceUnit.QueueReady) ;
            dataByteLow = (byte)busInterfaceUnit.GetNextInstructionByte();
            while (!busInterfaceUnit.QueueReady) ;
            dataByteHigh = (byte)busInterfaceUnit.GetNextInstructionByte();
        }

        private void fetch_address_low()
        {
            while (!busInterfaceUnit.QueueReady) ;
            addressLow = (byte)busInterfaceUnit.GetNextInstructionByte();
        }

        private void fetch_address_high()
        {
            while (!busInterfaceUnit.QueueReady) ;
            addressHigh = (byte)busInterfaceUnit.GetNextInstructionByte();
        }

        private void assign_register_word(ushort displacement)
        {
            registers[(WordGeneral)reg] = busInterfaceUnit.FetchDataWord(displacement);
        }

        private void assign_register_byte(ushort displacement)
        {
            registers[(ByteGeneral)reg] = busInterfaceUnit.FetchDataByte(displacement);
        }

        private void assign_register_word_immediate()
        {
            registers[(WordGeneral)reg] = (ushort)((dataByteHigh << 8) | dataByteLow);
        }

        private void assign_register_byte_immediate()
        {
            registers[(ByteGeneral)reg] = dataByteLow;
        }
        
        private void assign_register_register_byte()
        {
            registers[(ByteGeneral)reg] = registers[(ByteGeneral)rm];
        }

        private void assign_register_register_word()
        {
            registers[(WordGeneral)reg] = registers[(WordGeneral)rm];
        }

        private ushort get_displacement_from_mod()
        {
            switch (mod)
            {
                case 0:
                    if (rm == 0b110)
                    {
                        fetch_address_low();
                        fetch_address_high();
                        return (ushort)((addressHigh << 8) | addressLow);
                    }
                    else
                    {
                        return 0;
                    }
                case 1:
                    //fetch low address
                    fetch_address_low();
                    //cast to ushort
                    return addressLow;
                case 2:
                    //fetch low and high
                    fetch_address_low();
                    fetch_address_high();
                    return (ushort)((addressHigh << 8) | addressLow);
                default:
                    return 0;
            }
        }

        private ushort get_displacement_from_rm()
        {
            switch (rm)
            {
                case 0:
                    return (ushort)(registers.BX + registers.SI);
                case 1:
                    return (ushort)(registers.BX + registers.DI);
                case 2:
                    return (ushort)(registers.BP + registers.SI);
                case 3:
                    return (ushort)(registers.BP + registers.SI);
                case 4:
                    return registers.SI;
                case 5:
                    return registers.DI;
                case 6:
                    if (mod != 0)
                    {
                        return registers.BP;
                    }
                    else
                    {
                        return 0;
                    }
                case 7:
                    return registers.BX;
                default:
                    return 0;
            }
        }

        public ushort get_effective_address()
        {
            ushort displacement = get_displacement_from_rm();
            return (ushort)(displacement + get_displacement_from_mod());
        }

        private void zeroize()
        {
            opcode = 0;
            direction = false;
            width = false;
            mod = 0;
            reg = 0;
            rm = 0;
            dataByteLow = 0;
            dataByteHigh = 0;
            addressLow = 0;
            addressHigh = 0;
            tempByte = 0;
            tempWord = 0;
        }
    }
}
