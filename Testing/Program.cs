using CPU.i8088;
using CPU.SystemClock;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Testing
{
    class Program
    {
        static int ticks;
#pragma warning disable IDE1006 // Naming Styles
        static void Main(string[] args)
#pragma warning restore IDE1006 // Naming Styles
        {
            MainTimer timer = MainTimer.GetInstance();

            Processor cpu;

            Processor.BusInterfaceUnit biu = new Processor.BusInterfaceUnit();

            biu.SetMemory(0, 0xb0);
            biu.SetMemory(1, 0x28);
            biu.SetMemory(2, 0x02);
            biu.SetMemory(3, 0x06);
            biu.SetMemory(4, 0x20);
            biu.SetMemory(5, 0x00);
            biu.SetMemory(6, 0xb3);
            biu.SetMemory(7, 0x19);
            biu.SetMemory(8, 0x00);
            biu.SetMemory(9, 0xd8);
            biu.SetMemory(10, 0xb8);
            biu.SetMemory(11, 0x28);
            biu.SetMemory(12, 0x00);
            biu.SetMemory(13, 0x03);
            biu.SetMemory(14, 0x06);
            biu.SetMemory(15, 0x22);
            biu.SetMemory(16, 0x00);
            biu.SetMemory(17, 0xbb);
            biu.SetMemory(18, 0x1e);
            biu.SetMemory(19, 0x00);
            biu.SetMemory(20, 0x01);
            biu.SetMemory(21, 0xd8);
            biu.SetMemory(0x20, 30);
            biu.SetMemory(0x22, 60);

            cpu = new Processor(biu);

            timer.TockEvent += (sender, e) => { Console.WriteLine($"{timer.TotalTicks}"); };

            while (ticks < 1000) ;


            //Oscillator.InitClock();
            //Oscillator.Tick += on_tick;
            //RegisterSet regs = new RegisterSet();
            //BusInterfaceUnit biu = new BusInterfaceUnit(regs);

            //biu.SetMemory(0, 0b11000111);
            //biu.SetMemory(1, 0b00000110);
            //biu.SetMemory(2, 0x00);
            //biu.SetMemory(3, 0x10);
            //biu.SetMemory(4, 0x30);
            //biu.SetMemory(5, 0x40);


            //biu.WatchTicks();
            //ExecutionUnit test = new ExecutionUnit(regs,biu);
            

            //while(ticks < 40) { }
        }

        static void on_tick(object sender, EventArgs e)
        {
        //    ticks++;
        //    Console.WriteLine("Tick");
        //    if(ticks == 30)
        //    {
        //        Console.WriteLine("Done, press any key to exit");
        //        Oscillator.EndClock();
        //        Console.ReadKey();
        //        Environment.Exit(0);
        //    }
        }
    }
}
