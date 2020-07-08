using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Sources;

namespace PipeFrame
{
    public class Pipe<T>
    {
        private ManualResetValueTaskSource<T> back;
        private ManualResetValueTaskSource<T> into;

        public Pipe()
        {
            back = new ManualResetValueTaskSource<T>();
            into = new ManualResetValueTaskSource<T>();
            back.SetResult(default!);
        }

        public void Reset()
        {
            back.Reset();
            into.Reset();
            back.SetResult(default!);
        }

        public ValueTaskSourceStatus GetIntoStatus()
        {
            return into.GetStatus(into.Version);
        }


        public ValueTask<T> Into(T result)
        {            
            into.Reset();

            if (back.GetStatus(back.Version) == ValueTaskSourceStatus.Pending)
                back.SetResult(result);

            return new ValueTask<T>(into, into.Version);
        }

        public ValueTask<T> Back(T result)
        {
            if (back.GetStatus(back.Version) != ValueTaskSourceStatus.Pending)
                back.Reset();
            else
                throw new InvalidOperationException("back is Pending");

            if (into.GetStatus(into.Version) == ValueTaskSourceStatus.Pending)
                into.SetResult(result);

            return new ValueTask<T>(back, back.Version);
        }

    }
}
