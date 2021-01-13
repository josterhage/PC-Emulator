using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PC_Emulator
{
    /// <summary>
    /// Interaction logic for MemoryRowControl.xaml
    /// </summary>
    public partial class MemoryRowControl : UserControl
    {
        public MemoryRowControl()
        {
            InitializeComponent();
        }

        public string this[int i]
        {
            get
            {
                return i switch
                {
                    0 => zero.Text,
                    1 => one.Text,
                    2 => two.Text,
                    3 => three.Text,
                    4 => four.Text,
                    5 => five.Text,
                    6 => six.Text,
                    7 => seven.Text,
                    8 => eight.Text,
                    9 => nine.Text,
                    10 => a.Text,
                    11 => b.Text,
                    12 => c.Text,
                    13 => d.Text,
                    14 => e.Text,
                    15 => f.Text,
                    _ => "",
                };
            }
            set
            {
                switch (i)
                {
                    case 0:
                        zero.Text = value;
                        break;
                    case 1:
                        one.Text = value;
                        break;
                    case 2:
                        two.Text = value;
                        break;
                    case 3:
                        three.Text = value;
                        break;
                    case 4:
                        four.Text = value;
                        break;
                    case 5:
                        five.Text = value;
                        break;
                    case 6:
                        six.Text = value;
                        break;
                    case 7:
                        seven.Text = value;
                        break;
                    case 8:
                        eight.Text = value;
                        break;
                    case 9:
                        nine.Text = value;
                        break;
                    case 10:
                        a.Text = value;
                        break;
                    case 11:
                        b.Text = value;
                        break;
                    case 12:
                        c.Text = value;
                        break;
                    case 13:
                        d.Text = value;
                        break;
                    case 14:
                        e.Text = value;
                        break;
                    case 15:
                        f.Text = value;
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
