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

        private readonly Lazy<ConcurrentDictionary<string, SyncFrameScheduler>> groupFrame;

        public ConcurrentDictionary<string, SyncFrameScheduler> GroupFrame => groupFrame.Value;

        public PipFrameSystem(ILogger? logger,uint rate)
        {            
            this.logger = logger;
            this.Rate = rate;
            groupFrame = new Lazy<ConcurrentDictionary<string, SyncFrameScheduler>>(true);
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
            if (GroupFrame.ContainsKey(frame.GroupName))
            {
                GroupFrame[frame.GroupName].AddFrame(frame);
            }
            else
            {
                GroupFrame[frame.GroupName] = new SyncFrameScheduler(this.logger, this.Rate);
                GroupFrame[frame.GroupName].AddFrame(frame);

                bool isStart = false;
                foreach (var scheduler in GroupFrame.Values)
                {
                    if(scheduler.Status== SyncFrameScheduler.Runing)
                    {
                        isStart = true;                        
                    }
                }

                if (isStart)
                {
                    GroupFrame[frame.GroupName].Start();
                }
            }
        }

        /// <summary>
        /// 删除帧
        /// </summary>
        /// <param name="frame"></param>
        public void RemoveFrame(BaseFrame frame)
        {
            if (GroupFrame.ContainsKey(frame.GroupName))
            {
                GroupFrame[frame.GroupName].RemoveFrame(frame);
            }           
        }

        /// <summary>
        /// 启动
        /// </summary>
        public void Start()
        {
            foreach (var scheduler in GroupFrame.Values)
            {
                scheduler.Start();
            }
        }

        /// <summary>
        /// 停止
        /// </summary>
        public void Stop()
        {
            foreach (var scheduler in GroupFrame.Values)
            {
                scheduler.Stop();
            }
        }


    }
}
