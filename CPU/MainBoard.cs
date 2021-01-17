using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SystemBoard.i8088;
using SystemBoard.i8259;
using SystemBoard.Memory;
using SystemBoard.Bus;
using SystemBoard.SystemClock;
using SystemBoard.Keyboard;
using SystemBoard.i8255;

namespace SystemBoard
{
    public class MainBoard
    {
        public event EventHandler<MemoryChangeEventArgs> MemoryChangeEvent;
        public event EventHandler<SegmentChangeEventArgs> SegmentChangeEvent;
        public event EventHandler<InstructionPointerChangeEventArgs> InstructionPointerChangeEvent;
        public event EventHandler<GeneralRegisterChangeEventArgs> GeneralRegisterChangeEvent;
        public event EventHandler<FlagChangeEventArgs> FlagChangeEvent;
        public event EventHandler<PcKeyEventArgs> PcKeyEvent;

        private readonly Processor Cpu;
        private readonly FrontSideBusController frontSideBus;
        private readonly MemoryBusController memoryBus;
        private readonly IOBusController ioBus;
        private readonly RomChip programRom;
        private readonly InterruptController interruptController;
        private readonly PeripheralInterface peripheralInterface;

        //private readonly List<IMemoryLocation> memoryLocations;

        public MainBoard(IKeyboardConverter keyboardConverter)
        {
            memoryBus = new MemoryBusController();
            ioBus = new IOBusController();
            frontSideBus = new FrontSideBusController(memoryBus, ioBus);
            Cpu = new Processor(frontSideBus);
            programRom = new RomChip(0xfe000, @"C:\Users\joste\OneDrive\Documents\Code\5150 Emulator\asmscratch\one.o");
            interruptController = new InterruptController(Cpu, frontSideBus, 0x20);
            peripheralInterface = new PeripheralInterface(new KeyboardController(interruptController));

            Cpu.SegmentChangeEvent += OnSegmentChangeEvent;
            Cpu.GeneralRegisterChangeEvent += OnGeneralRegisterChangeEvent;
            Cpu.InstructionPointerChangeEvent += OnInstructionPointerChangeEvent;
            Cpu.FlagRegisterChangeEvent += OnFlagChangeEvent;
            Cpu.InterruptController = interruptController;

            memoryBus.Register(programRom);
            memoryBus.MemoryChangeEvent += OnMemoryChangeEvent;

            ioBus.Register(interruptController);
            ioBus.Register(peripheralInterface);

            keyboardConverter.PcKeyEvent += OnPcKeyEvent;
            PcKeyEvent += peripheralInterface.OnPcKeyEvent;
        }

        private void setup_ram(ushort dipswitchWord)
        {
            byte boardRam = (byte)((dipswitchWord & 12) >> 2);
            for (int i = 0; i <= boardRam; i++)
            {
                memoryBus.Register(new DramChip(i * 16384));
            }

            if (boardRam < 3)
                return;

            byte expansionRam = (byte)((dipswitchWord & 0x0f00) >> 8);

            for(int i = 0; i <= expansionRam; i++)
            {
                memoryBus.Register(new ExpansionRam(65536 + (i * 32768)));
            }
        }

        public void Start(ushort optionSwitches)
        {
            peripheralInterface.SetSwitches(optionSwitches);
            setup_ram(optionSwitches);
            Cpu.Start();
        }

        public void Stop()
        {
            Cpu.Stop();
        }

        protected void OnPcKeyEvent(object sender, PcKeyEventArgs e)
        {
            PcKeyEvent?.Invoke(sender, e);
        }

        protected void OnMemoryChangeEvent(object sender, MemoryChangeEventArgs e)
        {
            MemoryChangeEvent?.Invoke(sender, e);
        }

        protected void OnSegmentChangeEvent(object sender, SegmentChangeEventArgs e)
        {
            e.SegmentMap = memoryBus.GetSegmentMap(e.Value << 4);
            SegmentChangeEvent?.Invoke(sender, e);
        }

        protected void OnGeneralRegisterChangeEvent(object sender, GeneralRegisterChangeEventArgs e)
        {
            GeneralRegisterChangeEvent?.Invoke(sender, e);
        }

        protected void OnInstructionPointerChangeEvent(object sender, InstructionPointerChangeEventArgs e)
        {
            InstructionPointerChangeEvent?.Invoke(sender, e);
        }

        protected void OnFlagChangeEvent(object sender, FlagChangeEventArgs e)
        {
            FlagChangeEvent?.Invoke(sender, e);
        }

        public byte[] GetMemoryMap()
        {
            return memoryBus.GetMemoryMap();
        }

        public byte[] GetSegmentMap(int baseAddress)
        {
            return memoryBus.GetSegmentMap(baseAddress);
        }

        public Tuple<SegmentRegisters, ushort, GeneralRegisters, FlagRegister> GetRegisters()
        {
            return Cpu.GetRegisters();
        }
    }
}
