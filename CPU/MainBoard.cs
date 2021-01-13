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
        private readonly DramChip baseRam;
        private readonly InterruptController interruptController;
        
        //private readonly List<IMemoryLocation> memoryLocations;

        public MainBoard(IKeyboardConverter keyboardConverter)
        {
            keyboardConverter.PcKeyEvent += OnPcKeyEvent;

            baseRam = new DramChip(0);
            programRom = new RomChip(0xfe000, @"C:\Users\joste\OneDrive\Documents\Code\5150 Emulator\asmscratch\one.o");
            memoryBus = new MemoryBusController();
            memoryBus.Register(baseRam);
            memoryBus.Register(programRom);
            memoryBus.MemoryChangeEvent += OnMemoryChangeEvent;

            ioBus = new IOBusController();
            interruptController = new InterruptController(Cpu, frontSideBus, 0x20);
            ioBus.Register(interruptController);


            frontSideBus = new FrontSideBusController(memoryBus, ioBus);

            Cpu = new Processor(frontSideBus);
            Cpu.SegmentChangeEvent += OnSegmentChangeEvent;
            Cpu.GeneralRegisterChangeEvent += OnGeneralRegisterChangeEvent;
            Cpu.InstructionPointerChangeEvent += OnInstructionPointerChangeEvent;
            Cpu.FlagRegisterChangeEvent += OnFlagChangeEvent;
            Cpu.InterruptController = interruptController;
        }

        public void Start()
        {
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
            SegmentChangeEvent?.Invoke(sender,e);
        }

        protected void OnGeneralRegisterChangeEvent(object sender, GeneralRegisterChangeEventArgs e)
        {
            GeneralRegisterChangeEvent?.Invoke(sender, e);
        }

        protected void OnInstructionPointerChangeEvent(object sender, InstructionPointerChangeEventArgs e)
        {
            InstructionPointerChangeEvent?.Invoke(sender, e);
        }

        protected void OnFlagChangeEvent(object sender,FlagChangeEventArgs e)
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
