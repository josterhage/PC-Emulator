using System;

namespace CPU.i8088
{
    public partial class Processor
    {
        //TODO: after testing make this 'internal'
        //TODO: make exception messages meaningful
        private class FlagRegister
        {
            private bool cFlag; // 0b000000001 0
            private bool pFlag; // 0b000000010 1
            private bool aFlag; // 0b000000100 2
            private bool zFlag; // 0b000001000 3
            private bool sFlag; // 0b000010000 4
            private bool oFlag; // 0b000100000 5
            private bool iFlag; // 0b001000000 6
            private bool dFlag; // 0b010000000 7
            private bool tFlag; // 0b100000000 8

            public bool CF
            {
                get => cFlag; set => cFlag = value;
            }
            public bool PF
            {
                get => pFlag; set => pFlag = value;
            }
            public bool AF
            {
                get => aFlag; set => aFlag = value;
            }
            public bool ZF
            {
                get => zFlag; set => zFlag = value;
            }
            public bool SF
            {
                get => sFlag; set => sFlag = value;
            }
            public bool OF
            {
                get => oFlag; set => oFlag = value;
            }
            public bool IF
            {
                get => iFlag; set => iFlag = value;
            }
            public bool DF
            {
                get => dFlag; set => dFlag = value;
            }
            public bool TF
            {
                get => tFlag; set => tFlag = value;
            }

            public bool this[int i]
            {
                get
                {
                    switch (i)
                    {
                        case 0:
                            return cFlag;
                        case 1:
                            return pFlag;
                        case 2:
                            return aFlag;
                        case 3:
                            return zFlag;
                        case 4:
                            return sFlag;
                        case 5:
                            return oFlag;
                        case 6:
                            return iFlag;
                        case 7:
                            return dFlag;
                        case 8:
                            return tFlag;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                set
                {
                    switch (i)
                    {
                        case 0:
                            cFlag = value;
                            break;
                        case 1:
                            pFlag = value;
                            break;
                        case 2:
                            aFlag = value;
                            break;
                        case 3:
                            zFlag = value;
                            break;
                        case 4:
                            sFlag = value;
                            break;
                        case 5:
                            oFlag = value;
                            break;
                        case 6:
                            iFlag = value;
                            break;
                        case 7:
                            dFlag = value;
                            break;
                        case 8:
                            tFlag = value;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }

            public ushort Flags
            {
                get
                {
                    ushort result = 0;
                    result |= cFlag ? 1 : 0;
                    result |= pFlag ? 2 : 0;
                    result |= aFlag ? 4 : 0;
                    result |= zFlag ? 8 : 0;
                    result |= sFlag ? 16 : 0;
                    result |= oFlag ? 32 : 0;
                    result |= iFlag ? 64 : 0;
                    result |= dFlag ? 128 : 0;
                    result |= tFlag ? 256 : 0;
                    return result;
                }
            }
        }
    }
}
