using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPU.i8088
{
    public partial class Processor
    {
        #region enumerations
        public enum WordGeneral
        {
            AX, CX, DX, BX, SP, BP, SI, DI
        }

        public enum ByteGeneral
        {
            AL, CL, DL, BL, AH, CH, DH, BH
        }
        #endregion

        private class GeneralRegisters
        {
            // General purpose registers
            #region AX
            private byte ah = 0;
            private byte al = 0;

            public byte AH
            {
                get => ah;
                set => ah = value;
            }

            public byte AL
            {
                get => al;
                set => al = value;
            }

            public ushort AX
            {
                get
                {
                    return (ushort)((ah << 8) | al);
                }
                set
                {
                    ah = (byte)((value & 0xFF00) >> 8);
                    al = (byte)(value & 0x00FF);
                }
            }
            #endregion

            #region BX
            private byte bh = 0;
            private byte bl = 0;

            public byte BH
            {
                get => bh;
                set => bh = value;
            }

            public byte BL
            {
                get => bl;
                set => bl = value;
            }

            public ushort BX
            {
                get
                {
                    return (ushort)((bh << 8) | bl);
                }
                set
                {
                    bh = (byte)((value & 0xFF00) >> 8);
                    bl = (byte)(value & 0x00FF);
                }
            }
            #endregion

            #region CX
            private byte ch = 0;
            private byte cl = 0;

            public byte CH
            {
                get => ch;
                set => ch = value;
            }

            public byte CL
            {
                get => cl;
                set => cl = value;
            }

            public ushort CX
            {
                get
                {
                    return (ushort)((ch << 8) | cl);
                }
                set
                {
                    ch = (byte)((value & 0xFF00) >> 8);
                    cl = (byte)(value & 0x00FF);
                }
            }
            #endregion

            #region DX
            private byte dh = 0;
            private byte dl = 0;

            public byte DH
            {
                get => dh;
                set => dh = value;
            }

            public byte DL
            {
                get => dl;
                set => dl = value;
            }

            public ushort DX
            {
                get
                {
                    return (ushort)((dh << 8) | dl);
                }
                set
                {
                    dh = (byte)((value & 0xFF00) >> 8);
                    dl = (byte)(value & 0x00FF);
                }
            }
            #endregion

            #region POINTERS
            private ushort sp = 0;

            public ushort SP
            {
                get => sp;
                set => sp = value;
            }

            private ushort bp = 0;

            public ushort BP
            {
                get => bp;
                set => bp = value;
            }
            #endregion

            #region INDEXES
            private ushort si = 0;


            public ushort SI
            {
                get => si;
                set => si = value;
            }

            private ushort di = 0;

            public ushort DI
            {
                get => di;
                set => di = value;
            }
            #endregion

            #region word_indexers
            public ushort this[WordGeneral reg]
            {
                get
                {
                    switch (reg)
                    {
                        case WordGeneral.AX:
                            return AX;
                        case WordGeneral.CX:
                            return CX;
                        case WordGeneral.DX:
                            return DX;
                        case WordGeneral.BX:
                            return BX;
                        case WordGeneral.SP:
                            return sp;
                        case WordGeneral.BP:
                            return bp;
                        case WordGeneral.SI:
                            return si;
                        case WordGeneral.DI:
                            return di;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                set
                {
                    switch (reg)
                    {
                        case WordGeneral.AX:
                            AX = value;
                            return;
                        case WordGeneral.CX:
                            CX = value;
                            return;
                        case WordGeneral.DX:
                            DX = value;
                            return;
                        case WordGeneral.BX:
                            BX = value;
                            return;
                        case WordGeneral.SP:
                            sp = value;
                            return;
                        case WordGeneral.BP:
                            bp = value;
                            return;
                        case WordGeneral.SI:
                            si = value;
                            return;
                        case WordGeneral.DI:
                            di = value;
                            return;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }
            #endregion

            #region byte_indexers
            public byte this[ByteGeneral reg]
            {
                get
                {
                    switch (reg)
                    {
                        case ByteGeneral.AL:
                            return al;
                        case ByteGeneral.CL:
                            return cl;
                        case ByteGeneral.DL:
                            return dl;
                        case ByteGeneral.BL:
                            return bl;
                        case ByteGeneral.AH:
                            return ah;
                        case ByteGeneral.CH:
                            return ch;
                        case ByteGeneral.DH:
                            return dh;
                        case ByteGeneral.BH:
                            return bh;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                set
                {
                    switch (reg)
                    {
                        case ByteGeneral.AL:
                            al = value;
                            return;
                        case ByteGeneral.CL:
                            cl = value;
                            return;
                        case ByteGeneral.DL:
                            dl = value;
                            return;
                        case ByteGeneral.BL:
                            bl = value;
                            return;
                        case ByteGeneral.AH:
                            ah = value;
                            return;
                        case ByteGeneral.CH:
                            ch = value;
                            return;
                        case ByteGeneral.DH:
                            dh = value;
                            return;
                        case ByteGeneral.BH:
                            bh = value;
                            return;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }
            #endregion
        }
    }
}
