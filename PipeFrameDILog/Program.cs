using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using PipeFrame;

namespace PipeFrameDILog
{
    class TestFrame : BaseFrame
    {
        public TestFrame(ILogger logger):
            base(logger)
        {
            Priority = 2;
        }

        protected override async Task Execute()
        {
            while (true)
            {
                await NextFrame(10);

                logger.LogInformation("---1");
            }
        }

        public override void Cancel()
        {
            logger.LogInformation("TestFrame is Cancel");
        }
    }


    class TestFrame2 : BaseFrame
    {
        public TestFrame2(ILogger logger):
            base(logger)
        {
            Priority = 0;
        }

        protected override async Task Execute()
        {
            while (true)
            {
                var c = await NextFrame(20);
                logger.LogInformation("-------2-->" + c);

            }
        }
    }

    class TestFrame3 : BaseFrame
    {
        Stopwatch watch { get; set; } = new Stopwatch();

        public TestFrame3(ILogger logger) :
            base(logger,"T2")
        {
            Priority = 3;
        }

        protected override async Task Execute()
        {
            while (true)
            {

                var c = await NextFrame();
                logger.LogInformation("-------3--" + c);
                watch.Restart();
                await Task.Delay(3000);
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var diobj = new ServiceCollection();
            diobj.AddLogging(p => p.AddConsole());
            var di= diobj.BuildServiceProvider();
            using var loggerFactory= di.GetService<ILoggerFactory>();

            var log = loggerFactory.CreateLogger("frame system");

            var framesystem = new PipFrameSystem(log, 60);
            var frame = new TestFrame(log);
            framesystem.AddFrame(frame);
            framesystem.AddFrame(new TestFrame2(log));
            framesystem.AddFrame(new TestFrame3(log));
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
