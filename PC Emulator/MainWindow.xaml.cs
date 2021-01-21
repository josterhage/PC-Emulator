using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using SystemBoard;
using SystemBoard.Bus;
using SystemBoard.i8088;
using SystemBoard.SystemClock;

namespace PC_Emulator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MainBoard motherboard;
        private readonly KeyboardConverter keyboardConverter;
        private readonly MainTimer systemTimer = MainTimer.GetInstance();
        private int es;
        private int ss;
        private int cs;
        private int ds;

        //private MemoryViewController[] memoryViewControllers = new MemoryViewController[5];
        //private MemoryViewController csController;
        //private MemoryViewController dsController;
        //private MemoryViewController ssController;
        //private MemoryViewController esController;
        //private MemoryViewController ioController;

        public MainWindow()
        {
            InitializeComponent();

            keyboardConverter = new KeyboardConverter();

            KeyDown += keyboardConverter.OnWindowsKeyEvent;
            KeyUp += keyboardConverter.OnWindowsKeyEvent;

            motherboard = new MainBoard(keyboardConverter);
            motherboard.SegmentChangeEvent += OnSegmentChange;
            motherboard.GeneralRegisterChangeEvent += OnGeneralRegisterChange;
            motherboard.InstructionPointerChangeEvent += OnInstructionPointerChange;
            motherboard.FlagChangeEvent += OnFlagChange;

            initialize_text();

            //memoryViewControllers[0] = new MemoryViewController(ESListView, motherboard.GetSegmentMap(es << 4));
            //memoryViewControllers[1] = new MemoryViewController(SSListView, motherboard.GetSegmentMap(ss << 4));
            //memoryViewControllers[2] = new MemoryViewController(CSListView, motherboard.GetSegmentMap(cs << 4));
            //memoryViewControllers[3] = new MemoryViewController(DSListView, motherboard.GetSegmentMap(ds << 4));
            //memoryViewControllers[4] = new MemoryViewController(IOListView, motherboard.GetSegmentMap(0));
        }

        private void initialize_text()
        {
            var regs = motherboard.GetRegisters();

            RegisterCS.Text = $"{regs.Item1.CS:X04}";
            RegisterSS.Text = $"{regs.Item1.SS:X04}";
            RegisterDS.Text = $"{regs.Item1.DS:X04}";
            RegisterES.Text = $"{regs.Item1.ES:X04}";
            RegisterIP.Text = $"{regs.Item2:X04}";

            RegisterAX.Text = $"{regs.Item3.AX:X04}";
            RegisterBX.Text = $"{regs.Item3.BX:X04}";
            RegisterCX.Text = $"{regs.Item3.CX:X04}";
            RegisterDX.Text = $"{regs.Item3.DX:X04}";

            RegisterBP.Text = $"{regs.Item3.BP:X04}";
            RegisterSP.Text = $"{regs.Item3.SP:X04}";
            RegisterSI.Text = $"{regs.Item3.SI:X04}";
            RegisterDI.Text = $"{regs.Item3.DI:X04}";

            TFBox.IsChecked = regs.Item4.TF;
            DFBox.IsChecked = regs.Item4.DF;
            IFBox.IsChecked = regs.Item4.IF;
            OFBox.IsChecked = regs.Item4.OF;
            SFBox.IsChecked = regs.Item4.SF;
            ZFBox.IsChecked = regs.Item4.ZF;
            AFBox.IsChecked = regs.Item4.AF;
            PFBox.IsChecked = regs.Item4.PF;
            CFBox.IsChecked = regs.Item4.CF;

            es = regs.Item1.ES;
            ss = regs.Item1.SS;
            cs = regs.Item1.CS;
            ds = regs.Item1.DS;
        }

        public void OnSegmentChange(object sender, SegmentChangeEventArgs e)
        {
            switch (e.Segment)
            {
                case Segment.CS:
                    cs = e.Value;
                    Dispatcher.Invoke(() =>
                    {
                        RegisterCS.Text = $"{e.Value:X04}";
                    });
                    break;
                case Segment.DS:
                    ds = e.Value;
                    Dispatcher.Invoke(() =>
                    {
                        RegisterDS.Text = $"{e.Value:X04}";
                    });
                    break;
                case Segment.ES:
                    es = e.Value;
                    Dispatcher.Invoke(() =>
                    {
                        RegisterES.Text = $"{e.Value:X04}";
                    });
                    break;
                case Segment.SS:
                    ss = e.Value;
                    Dispatcher.Invoke(() =>
                    {
                        RegisterSS.Text = $"{e.Value:X04}";
                    });
                    break;
            }

            //Dispatcher.Invoke(() =>
            //{
            //    memoryViewControllers[(int)e.Segment].ReplaceAll(e.SegmentMap);
            //});
        }

        public void OnGeneralRegisterChange(object sender, GeneralRegisterChangeEventArgs e)
        {
            switch (e.Register)
            {
                case WordGeneral.AX:
                    Dispatcher.Invoke(() =>
                    {
                        RegisterAX.Text = $"{e.Value:X04}";
                    });
                    break;
                case WordGeneral.BX:
                    Dispatcher.Invoke(() =>
                    {
                        RegisterBX.Text = $"{e.Value:X04}";
                    });
                    break;
                case WordGeneral.CX:
                    Dispatcher.Invoke(() =>
                    {
                        RegisterCX.Text = $"{e.Value:X04}";
                    });
                    break;
                case WordGeneral.DX:
                    Dispatcher.Invoke(() =>
                    {
                        RegisterDX.Text = $"{e.Value:X04}";
                    });
                    break;
                case WordGeneral.SP:
                    Dispatcher.Invoke(() =>
                    {
                        RegisterSP.Text = $"{e.Value:X04}";
                    });
                    break;
                case WordGeneral.BP:
                    Dispatcher.Invoke(() =>
                    {
                        RegisterBP.Text = $"{e.Value:X04}";
                    });
                    break;
                case WordGeneral.SI:
                    Dispatcher.Invoke(() =>
                    {
                        RegisterSI.Text = $"{e.Value:X04}";
                    });
                    break;
                case WordGeneral.DI:
                    Dispatcher.Invoke(() =>
                    {
                        RegisterDI.Text = $"{e.Value:X04}";
                    });
                    break;
            }
        }

        public void OnInstructionPointerChange(object sender, InstructionPointerChangeEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                RegisterIP.Text = $"{e.IP:X04}";
            });
        }

        public void OnFlagChange(object sender, FlagChangeEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                TFBox.IsChecked = e.Flags.TF;
                DFBox.IsChecked = e.Flags.DF;
                IFBox.IsChecked = e.Flags.IF;
                OFBox.IsChecked = e.Flags.OF;
                SFBox.IsChecked = e.Flags.SF;
                ZFBox.IsChecked = e.Flags.ZF;
                AFBox.IsChecked = e.Flags.AF;
                PFBox.IsChecked = e.Flags.PF;
                CFBox.IsChecked = e.Flags.CF;
            });
        }

        public void OnMemorychange(object sender, MemoryChangeEventArgs e)
        {
            int offset;
            foreach (Segment s in location_in_segment(e.Location))
            {
                switch (s)
                {
                    case Segment.ES:
                        offset = e.Location - (es << 4);
                        //Dispatcher.Invoke(() =>
                        //{
                        //    memoryViewControllers[0].ReplaceOne(e.Value, offset);
                        //});
                        break;
                    case Segment.SS:
                        offset = e.Location - (ss << 4);
                        //Dispatcher.Invoke(() =>
                        //{
                        //    memoryViewControllers[1].ReplaceOne(e.Value, offset);
                        //});
                        break;
                    case Segment.CS:
                        offset = e.Location - (cs << 4);
                        //Dispatcher.Invoke(() =>
                        //{
                        //    memoryViewControllers[2].ReplaceOne(e.Value, offset);
                        //});
                        break;
                    case Segment.DS:
                        offset = e.Location - (ds << 4);
                        //Dispatcher.Invoke(() =>
                        //{
                        //    memoryViewControllers[3].ReplaceOne(e.Value, offset);
                        //});
                        break;
                }
            }

            //if (e.Location < 65536)
            //{
            //    Dispatcher.Invoke(() =>
            //    {
            //        memoryViewControllers[4].ReplaceOne(e.Value, e.Location);
            //    });
            //}
        }

        private Segment[] location_in_segment(int location)
        {
            Segment[] values = new Segment[1];
            int segs = 0;
            int seg;
            for (int i = 0; i < 4; i++)
            {
                seg = i switch
                {
                    0 => es << 4,
                    1 => cs << 4,
                    2 => ds << 4,
                    3 => ss << 4,
                    _ => 0,
                };
                if (seg < location && location < (seg + 65536))
                {
                    Segment[] temp = new Segment[segs + 1];
                    Array.Copy(values, temp, segs);
                    temp[segs] = (Segment)i;
                    values = temp;
                }
            }
            if (segs == 0)
                values[0] = Segment.none;
            return values;
        }

        //App cleanup
        protected override void OnClosing(CancelEventArgs e)
        {
            motherboard.Stop();
            base.OnClosing(e);
        }

        private void button_click(object sender, RoutedEventArgs e)
        {
            ushort switches = (byte)((bool)Ipl5.IsChecked ? 1 : 0);
            switches |= (byte)((bool)Ram0.IsChecked ? 4 : 0);
            switches |= (byte)((bool)Ram1.IsChecked ? 8 : 0);
            switches |= (byte)((bool)Display0.IsChecked ? 16 : 0);
            switches |= (byte)((bool)Display1.IsChecked ? 32 : 0);
            switches |= (byte)((bool)FloppyCount0.IsChecked ? 64 : 0);
            switches |= (byte)((bool)FloppyCount1.IsChecked ? 128 : 0);
            switches |= (byte)((bool)IoRam0.IsChecked ? 256 : 0);
            switches |= (byte)((bool)IoRam1.IsChecked ? 512 : 0);
            switches |= (byte)((bool)IoRam2.IsChecked ? 1024 : 0);
            switches |= (byte)((bool)IoRam3.IsChecked ? 2048 : 0);
            motherboard.Start(switches);
        }
    }
}
