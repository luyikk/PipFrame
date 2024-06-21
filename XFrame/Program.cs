using System;
using System.Net;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

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

    //public class SingleThreadSynchronizationContext : SynchronizationContext
    //{
    //    private readonly BlockingCollection<KeyValuePair<SendOrPostCallback, object>> queue =
    //        new BlockingCollection<KeyValuePair<SendOrPostCallback, object>>();

    //    public override void Post(SendOrPostCallback d, object state)
    //    {
    //        queue.Add(new KeyValuePair<SendOrPostCallback, object>(d, state));
    //    }

    //    public void RunOnCurrentThread()
    //    {
    //        KeyValuePair<SendOrPostCallback, object> workItem;
    //        while (queue.TryTake(out workItem, Timeout.Infinite))
    //            workItem.Key(workItem.Value);
    //    }

    //    public void Complete() => queue.CompleteAdding();
    //}

    //class Program
    //{
    //    static void Main(string[] args)
    //    {
    //        var context = new SingleThreadSynchronizationContext();
    //        SynchronizationContext.SetSynchronizationContext(context);

    //        Task.Factory.StartNew(AsyncOperation);

    //        context.RunOnCurrentThread();

    //        context.Complete();
    //    }

    //    static async void AsyncOperation()
    //    {
    //        Console.WriteLine("Async operation started on thread id " + Thread.CurrentThread.ManagedThreadId);
    //        await Task.Delay(1000);
    //        Console.WriteLine("Async operation finished on thread id " + Thread.CurrentThread.ManagedThreadId);
    //    }
    //}

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
