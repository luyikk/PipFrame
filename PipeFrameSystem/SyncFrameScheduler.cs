using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks.Sources;
using System.Transactions;
using Microsoft.Extensions.Logging;

namespace PipeFrame
{
   
    public class SyncFrameScheduler
    {
        public const int Idle = 0;
        public const int Runing = 1;
        public const int Stoping = 2;

        private readonly ILogger? logger;

        private int status;

        /// <summary>
        /// 状态
        /// </summary>
        public int Status => status;


        /// <summary>
        /// 帧率
        /// </summary>
        public uint Rate { get; private set; }


        /// <summary>
        /// 帧集合
        /// </summary>
        public List<BaseFrame> Frames { get; private set; }

        /// <summary>
        /// 添加帧集合
        /// </summary>
        public List<BaseFrame> AddFrames { get; private set; }

        /// <summary>
        /// 一帧需要运行时间
        /// </summary>
        private readonly long AverageTick;


        public SyncFrameScheduler(uint rate)
            :this(null,rate)
        {           
        }


        public SyncFrameScheduler(ILogger? logger,uint rate)
        {           
            Rate = rate;
            this.logger = logger;
            AverageTick = 10000000 / rate;
            Frames = new List<BaseFrame>();
            AddFrames = new List<BaseFrame>();
        }


        /// <summary>
        /// 添加帧
        /// </summary>
        /// <param name="frame"></param>
        public void AddFrame(BaseFrame frame)
        {
            if (!Frames.Contains(frame))
            {
                AddFrames.Add(frame);
            }
        }

        /// <summary>
        /// 删除帧
        /// </summary>
        /// <param name="frame"></param>
        public bool RemoveFrame(BaseFrame frame)
        {
            if (Frames.Contains(frame))
            {
                frame.IsRemove = true;
                return true;
            }
            return false;
        }

        /// <summary>
        /// 启动
        /// </summary>
        public void Start()
        {
            if (Interlocked.Exchange(ref status, Runing) == Idle)
            {
                Task.Factory.StartNew(Run);
            }
        }

        /// <summary>
        /// 停止
        /// </summary>
        public void Stop()
        {
            Interlocked.CompareExchange(ref status, Stoping, Runing);
        }

        private async void Run()
        {
            try
            {
                var stop = new Stopwatch();

                // 稳定帧回调用的时间池 |最后运行完成时间
                long ticksPool = 0;

                while (status is Runing)
                {

                    stop.Restart();

                    //删除帧
                    Frames.RemoveAll(p =>
                    {
                        if (p.IsRemove)
                        {
                            p.Cancel();
                            return true;
                        }
                        else
                            return false;
                    });

                    //添加帧
                    if (AddFrames.Count > 0)
                    {
                        foreach (var frame in AddFrames)
                        {
                            if (!Frames.Contains(frame))
                                Frames.Add(frame);
                        }

                        AddFrames.Clear();
                    }

                    Frames.Sort((a, b) => b.Priority.CompareTo(a.Priority));

                    //把所有帧取出
                    foreach (var current in Frames)
                    {
                        try
                        {
                            if (!current.IsWaitStatus)
                                current.Run();
                            else
                            {
                                var diff_time = Convert.ToInt32((DateTime.Now.Ticks - current.LastTicks) / 10000);
                                if (await current.Into(diff_time) == -99999)
                                    current.Reset();
                            }
                        }
                        catch (Exception er)
                        {
                            logger?.LogError(er, "run frame error");
                        }
                    }


                    var elapsedtick = stop.ElapsedTicks;
                    ticksPool += elapsedtick;
                    //等待时间
                    int waitMs;
                    if (ticksPool > AverageTick)
                    {
                        ticksPool -= AverageTick;
                        waitMs = 0;
                    }
                    else
                    {
                        waitMs = (int)(AverageTick - elapsedtick) / 10000;
                    }

                    await Task.Delay(waitMs);


                }
                Interlocked.CompareExchange(ref status, Idle, Stoping);
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "SynFrameScheduler Error");
            }
        }
    }
}
