using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SystemBoard.Memory
{
    public class RomChip : IMemoryLocation
    {
        private readonly string romFilePath;
        private readonly int _baseAddress;
        public int BaseAddress
        {
            get => _baseAddress;
        }

        private readonly byte[] data = new byte[8192];

        public RomChip(int baseAddress, string path)
        {
            _baseAddress = baseAddress;
            romFilePath = path;
            load_from_file(romFilePath);
        }

        public byte Read(int location)
        {
            return data[location - _baseAddress];
        }

        public void Write(int location, byte value)
        {
            return;
        }

        private void load_from_file(string path)
        {
            byte[] result;

            using (FileStream sourceStream = File.Open(path, FileMode.Open))
            {
                result = new byte[sourceStream.Length];
                sourceStream.Read(result, 0, (int)sourceStream.Length);
            }

            for (int i = 0; i < result.Length; i++)
            {
                data[i] = result[i];
            }
        }
    }
}