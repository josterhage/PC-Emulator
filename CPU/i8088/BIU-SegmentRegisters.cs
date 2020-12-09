using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPU.i8088
{
    public partial class Processor
    {
        private partial class BusInterfaceUnit
        {
            private enum Segment
            {
                ES,CS,SS,DS
            }
            private class SegmentRegisters
            {
                public ushort ES = 0;
                public ushort CS = 0;
                public ushort SS = 0;
                public ushort DS = 0;

                public ushort this[Segment i]
                {
                    get
                    {
                        switch (i)
                        {
                            case Segment.ES:
                                return ES;
                            case Segment.CS:
                                return CS;
                            case Segment.SS:
                                return SS;
                            case Segment.DS:
                                return DS;
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
                            case Segment.CS:
                                CS = value;
                                break;
                            case Segment.SS:
                                SS = value;
                                break;
                            case Segment.DS:
                                DS = value;
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }
                }
            }
        }
|}
}