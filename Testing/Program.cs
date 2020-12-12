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
            biu.SetMemory(1, 0x42);
            biu.SetMemory(2, 0xb3);
            biu.SetMemory(3, 0xbd);
            biu.SetMemory(4, 0x00);
            biu.SetMemory(5, 0xd8);

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
