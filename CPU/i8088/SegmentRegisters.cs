using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SystemBoard.i8088
{
    public enum Segment
    {
        ES, SS, CS, DS, absolute = 98, none = 99
    }

    public class SegmentRegisters
    {
        public event EventHandler SegmentChangeHandler;

        public ushort ES
        {
            get => ES;
            set
            {
                ES = value;
                SegmentChangeHandler?.Invoke(this, new EventArgs());
            }
        }

        public ushort SS
        {
            get => SS;
            set
            {
                SS = value;
                SegmentChangeHandler?.Invoke(this, new EventArgs());
            }
        }

        public ushort CS
        {
            get => CS;
            set
            {
                CS = value;
                SegmentChangeHandler?.Invoke(this, new EventArgs());
            }
        }

        public ushort DS
        {
            get => DS;
            set
            {
                DS = value;
                SegmentChangeHandler?.Invoke(this, new EventArgs());
            }
        }

        public SegmentRegisters()
        {
            ES = SS = DS = 0;
            CS = 0xFFF0;
        }

        public ushort this[Segment i]
        {
            get
            {
                switch (i)
                {
                    case Segment.ES:
                        return ES;
                    case Segment.SS:
                        return SS;
                    case Segment.CS:
                        return CS;
                    case Segment.DS:
                        return DS;
                    case Segment.absolute:
                        return 0;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            set
            {
                switch (i)
                {
                    case Segment.ES:
                        ES = value;
                        break;
                    case Segment.SS:
                        SS = value;
                        break;
                    case Segment.CS:
                        CS = value;
                        break;
                    case Segment.DS:
                        DS = value;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public SegmentRegisters Clone()
        {
            return new SegmentRegisters
            {
                ES = ES,
                SS = SS,
                CS = CS,
                DS = DS
            };
        }
    }
}
