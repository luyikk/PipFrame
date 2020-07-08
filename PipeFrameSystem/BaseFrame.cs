using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Sources;

namespace PipeFrame
{
    /// <summary>
    /// 脚本基类
    /// </summary>
    public abstract class BaseFrame
    {
        /// <summary>
        /// 一次逻辑运行完 设置的状态
        /// </summary>
        const int complete = -99999;

   
        /// <summary>
        /// 管道
        /// </summary>
        private readonly Pipe<int> pipe;

        /// <summary>
        /// 当前是否等待模式
        /// </summary>
        internal bool IsWaitStatus { get; private set; }


        /// <summary>
        /// 帧忽略,每忽略一次减1 直到0 开始正常运行
        /// </summary>
        protected ushort skipFrame;

        /// <summary>
        /// 优先级,越大越优先
        /// </summary>
        public int Priority { get; set; }

        /// <summary>
        /// 是否下一帧被删除
        /// </summary>
        public bool IsRemove { get; internal set; }

        /// <summary>
        /// 上一次运行片刻时
        /// </summary>
        public long LastTicks { get; private set; }


        /// <summary>
        /// 组名
        /// </summary>
        public string GroupName { get; }


        protected readonly ILogger? logger;

        public BaseFrame() : this(null, "default")
        {
           
        }

        public BaseFrame(ILogger? logger) : this(logger, "default")
        {

        }

        public BaseFrame(string groupname) : this(null, groupname)
        {

        }

        public BaseFrame(ILogger? logger,string groupname)
        {
            this.logger = logger;
            GroupName = groupname;
            pipe = new Pipe<int>();
        }


         /// <summary>
         /// 下一帧
         /// </summary>
         /// <param name="skip">忽略帧数</param>
         /// <returns>返回从上一次结束到现在经过的时间 单位毫秒</returns>
        protected ValueTask<int> NextFrame(ushort skip = 0)
        {
            skipFrame = skip;
            var wait = pipe.Back(skip);         
            LastTicks = DateTime.Now.Ticks;
            return wait;
        }


       

        /// <summary>
        /// 等待返回
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        internal ValueTask<int> Into(int value)
        {
            if (skipFrame > 1)
            {
                skipFrame--;
                return new ValueTask<int>(-2);
            }
            else
            {
                return pipe.Into(value);
            }
        }

     
        /// <summary>
        /// 重置
        /// </summary>
        internal void Reset()
        {
            pipe.Reset();
            skipFrame = 0;
            IsWaitStatus = false;  
        }

        /// <summary>
        /// 从容器里移除触发
        /// </summary>
        public virtual void Cancel()
        {

        }

        /// <summary>
        /// 运行
        /// </summary>
        internal async void Run()
        {
            try
            {
                IsWaitStatus = true;
                await Execute();
                IsWaitStatus = false;

                if (pipe.GetIntoStatus() == ValueTaskSourceStatus.Pending)
                    await pipe.Back(complete);

                LastTicks = DateTime.Now.Ticks;
            }
            catch (Exception er)
            {
                logger?.LogError(er, "BaseFrame Error");
            }
        }

        /// <summary>
        /// 继续运行
        /// </summary>
        /// <returns></returns>
        protected abstract Task Execute();
    }
}
