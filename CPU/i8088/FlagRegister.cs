using System;

namespace SystemBoard.i8088
{
    //TODO: make exception messages meaningful
    public class FlagRegister
    {
        public event EventHandler<FlagChangeEventArgs> FlagsChangeHandler;

        private bool cFlag; // 0b000000001 1
        private bool pFlag; // 4
        private bool aFlag; // 16
        private bool zFlag; // 64
        private bool sFlag; // 128
        private bool tFlag; // 256
        private bool iFlag; // 512
        private bool dFlag; // 1024
        private bool oFlag; // 2048

        public bool CF
        {
            get => cFlag;
            set
            {
                cFlag = value;
                FlagsChangeHandler?.Invoke(this, new FlagChangeEventArgs(this));
            }
        }
        public bool PF
        {
            get => pFlag;
            set
            {
                pFlag = value;
                FlagsChangeHandler?.Invoke(this, new FlagChangeEventArgs(this));
            }
        }
        public bool AF
        {
            get => aFlag; set
            {
                aFlag = value;
                FlagsChangeHandler?.Invoke(this, new FlagChangeEventArgs(this));
            }
        }
        public bool ZF
        {
            get => zFlag; set
            {
                zFlag = value;
                FlagsChangeHandler?.Invoke(this, new FlagChangeEventArgs(this));
            }
        }
        public bool SF
        {
            get => sFlag; set
            {
                sFlag = value;
                FlagsChangeHandler?.Invoke(this, new FlagChangeEventArgs(this));
            }
        }
        public bool TF
        {
            get => tFlag; set
            {
                tFlag = value;
                FlagsChangeHandler?.Invoke(this, new FlagChangeEventArgs(this));
            }
        }
        public bool IF
        {
            get => iFlag; set
            {
                iFlag = value;
                FlagsChangeHandler?.Invoke(this, new FlagChangeEventArgs(this));
            }
        }
        public bool DF
        {
            get => dFlag; set
            {
                dFlag = value;
                FlagsChangeHandler?.Invoke(this, new FlagChangeEventArgs(this));
            }
        }
        public bool OF
        {
            get => oFlag; set
            {
                oFlag = value;
                FlagsChangeHandler?.Invoke(this, new FlagChangeEventArgs(this));
            }
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
                result |= pFlag ? 4 : 0;
                result |= aFlag ? 16 : 0;
                result |= zFlag ? 64 : 0;
                result |= sFlag ? 128 : 0;
                result |= tFlag ? 256 : 0;
                result |= iFlag ? 512 : 0;
                result |= dFlag ? 1024 : 0;
                result |= oFlag ? 2048 : 0;
                return result;
            }
            set
            {
                cFlag = (value & 1) != 0;
                pFlag = (value & 4) != 0;
                aFlag = (value & 16) != 0;
                zFlag = (value & 64) != 0;
                sFlag = (value & 128) != 0;
                tFlag = (value & 256) != 0;
                iFlag = (value & 512) != 0;
                dFlag = (value & 1024) != 0;
                oFlag = (value & 2048) != 0;
            }
        }

        public FlagRegister Clone()
        {
            return new FlagRegister
            {
                cFlag = cFlag,
                pFlag = pFlag,
                aFlag = aFlag,
                zFlag = zFlag,
                sFlag = sFlag,
                tFlag = tFlag,
                iFlag = iFlag,
                dFlag = dFlag,
                oFlag = oFlag
            };
        }
    }
}
