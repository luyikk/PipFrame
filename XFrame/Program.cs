using System;
using System.Net;
using System.Threading.Tasks;
using System.Diagnostics;

namespace PipeFrame
{
    class TestFrame : BaseFrame
    {
        public TestFrame()
        {
            Priority = 2;
        }

        protected override async Task Execute()
        {       
            while (true)
            {
                await NextFrame(10);

                Console.WriteLine("---1");
            }
        }

        public override void Cancel()
        {
            Console.WriteLine("TestFrame is Cancel");
        }
    }


    class TestFrame2 : BaseFrame
    {
        public TestFrame2()
        {
            Priority = 0;
        }

        protected override async Task Execute()
        {
            while (true)
            {
                var c= await NextFrame(20);
                Console.WriteLine("-------2-->"+c);
               
            }
        }
    }

    class TestFrame3 : BaseFrame
    {
        Stopwatch watch { get; set; } = new Stopwatch();

        public TestFrame3() :
            base("T2")
        {
            Priority = 3;            
        }

        protected override async Task Execute()
        {
            while (true)
            {

                var c = await NextFrame();
                Console.WriteLine("-------3--" + c);
                watch.Restart();
                await Task.Delay(3000);
            }
        }
    }

    class Program
    {

        static void Main(string[] args)
        {

            var framesystem = new PipFrameSystem(60);

            var frame = new TestFrame();
            framesystem.AddFrame(frame);
            framesystem.AddFrame(new TestFrame2());
            framesystem.AddFrame(new TestFrame3());
            framesystem.Start();          
            Console.ReadLine();

            framesystem.RemoveFrame(frame);

            Console.ReadLine();

            while (true)
            {
                framesystem.Stop();
                Console.ReadLine();
                framesystem.Start();
                Console.ReadLine();
            }

        }
    }
}
