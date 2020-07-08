using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace PipeFrame
{
    public class PipFrameSystem
    {
        private readonly ILogger? logger;      
        public uint Rate { get; }

        private readonly Lazy<ConcurrentDictionary<string, SyncFrameScheduler>> groupFrames;

        public ConcurrentDictionary<string, SyncFrameScheduler> GroupFrames => groupFrames.Value;

        public PipFrameSystem(ILogger? logger,uint rate)
        {            
            this.logger = logger;
            this.Rate = rate;
            groupFrames = new Lazy<ConcurrentDictionary<string, SyncFrameScheduler>>(true);
        }

        public PipFrameSystem(uint rate)
            :this(null,rate)
        {
          
        }


        /// <summary>
        /// 添加帧
        /// </summary>
        /// <param name="frame"></param>
        public void AddFrame(BaseFrame frame)
        {
            if (GroupFrames.ContainsKey(frame.GroupName))
            {
                GroupFrames[frame.GroupName].AddFrame(frame);
            }
            else
            {
                GroupFrames[frame.GroupName] = new SyncFrameScheduler(this.logger, this.Rate);
                GroupFrames[frame.GroupName].AddFrame(frame);

                bool isStart = false;
                foreach (var scheduler in GroupFrames.Values)
                {
                    if(scheduler.Status== SyncFrameScheduler.Runing)
                    {
                        isStart = true;                        
                    }
                }

                if (isStart)
                {
                    GroupFrames[frame.GroupName].Start();
                }
            }
        }

        /// <summary>
        /// 删除帧
        /// </summary>
        /// <param name="frame"></param>
        public void RemoveFrame(BaseFrame frame)
        {
            if (GroupFrames.ContainsKey(frame.GroupName))
            {
                GroupFrames[frame.GroupName].RemoveFrame(frame);
            }           
        }

        /// <summary>
        /// 启动
        /// </summary>
        public void Start()
        {
            foreach (var scheduler in GroupFrames.Values)
            {
                scheduler.Start();
            }
        }

        /// <summary>
        /// 停止
        /// </summary>
        public void Stop()
        {
            foreach (var scheduler in GroupFrames.Values)
            {
                scheduler.Stop();
            }
        }


    }
}
