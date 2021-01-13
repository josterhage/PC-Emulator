using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SystemBoard.i8088
{
    public enum Segment
    {
        ES, SS, CS, DS, IO, none = 99
    }

    public class SegmentRegisters
    {
        public event EventHandler<SegmentChangeEventArgs> SegmentChangeHandler;

        private ushort _es;
        private ushort _ss;
        private ushort _cs;
        private ushort _ds;

        public ushort ES
        {
            get => _es;
            set
            {
                _es = value;
                SegmentChangeHandler?.Invoke(this, new SegmentChangeEventArgs(Segment.ES,_es,null));
            }
        }

        public ushort SS
        {
            get => _ss;
            set
            {
                _ss = value;
                SegmentChangeHandler?.Invoke(this, new SegmentChangeEventArgs(Segment.SS, _ss, null));
            }
        }

        public ushort CS
        {
            get => _cs;
            set
            {
                _cs = value;
                SegmentChangeHandler?.Invoke(this, new SegmentChangeEventArgs(Segment.CS, _cs, null));
            }
        }

        public ushort DS
        {
            get => _ds;
            set
            {
                _ds = value;
                SegmentChangeHandler?.Invoke(this, new SegmentChangeEventArgs(Segment.DS, _ds, null));
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
                    case Segment.IO:
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
                _es = _es,
                _ss = _ss,
                _cs = _cs,
                _ds = _ds
            };
        }
    }
}
