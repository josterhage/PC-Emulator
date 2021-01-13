using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;

namespace PC_Emulator
{
    public class MemoryViewController
    {
        private byte[] memoryMap = new byte[65536];
        private ListView listView;
        private MemoryRowControl[] memoryRowControls = new MemoryRowControl[4096];

        public MemoryViewController(ListView listView, byte[] startingMemory)
        {
            for (int i = 0; i < 4096; i++)
            {
                memoryRowControls[i] = new MemoryRowControl();
                listView.Items.Add(memoryRowControls[i]);
                memoryRowControls[i].offset.Text = $"{i * 16:X04}";
                string decode = "";
                for (int j = 0; j < 16; j++)
                {
                    memoryRowControls[i][j] = $"{startingMemory[(i * 16) + j]:X02}";
                    decode += (char)startingMemory[(i * 16) + j];
                }
                memoryRowControls[i].decoded.Text = decode;
            }
        }

        public void ReplaceAll(byte[] newMemory)
        {
            for(int i = 0; i < 4096; i++)
            {
                string decode = "";
                for(int j = 0; j < 16; j++)
                {
                    memoryRowControls[i][j] = $"{newMemory[(i * 16) + j]:X02}";
                    decode += (char)newMemory[(i * 16) + j];
                }
                memoryRowControls[i].decoded.Text = decode;
            }
        }

        public void ReplaceOne(byte newByte, int location)
        {
            if (location >= 65536)
                throw new ArgumentOutOfRangeException(nameof(location));
            int row = location / 16;
            int offset = location % 16;
            memoryRowControls[row][offset] = $"{newByte:X02}";
            string decode = "";
            for(int i = 0; i < 16; i++)
            {
                decode += i == offset ? (char)newByte : memoryRowControls[row].decoded.Text[i];
            }
            memoryRowControls[row].decoded.Text = decode;
        }
    }
}
