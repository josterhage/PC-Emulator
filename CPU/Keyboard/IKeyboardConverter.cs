using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SystemBoard.Keyboard
{
    public interface IKeyboardConverter
    {
        event EventHandler<PcKeyEventArgs> PcKeyEvent;
    }
}
