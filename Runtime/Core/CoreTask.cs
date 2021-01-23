using System;
using GGSharpData;
using GGSharpPool;
using GGSharpTick;

namespace GGSharpTask
{
    /// <summary>
    /// Implementation of ICoreTask interface
    /// </summary>
    public class CoreTask : CoreSystemBase<CoreTaskSystemConfigData, ICoreSystemClientTask>, ICoreTask
    {
        #region Variables

        private readonly ICoreTick _coreTick;
        
        private readonly IPool _taskPool;
        private readonly IPool _timerPool;
        
        #endregion Variables
        
        
        #region Construction

        public CoreTask(CoreTaskSystemConfigData data, ICoreSystemClientTask client) 
            : base(data, client)
        {
            // Set the tick system
            _coreTick = data.TickSystem;
            
            // Create pool of tasks
            _taskPool = new TaskPool(_coreTick);
        }

        #endregion Construction


        #region Tasks

        TaskHandle ICoreTask.Task(Action<float> action, ITicksetInstance tickset)
        {
            return (_taskPool.GetNext() as TaskHandle).Set(action, -1, tickset);
        }

        public TaskHandle Task(Action<float> action, float duration, ITicksetInstance tickset)
        {
            return (_taskPool.GetNext() as TaskHandle).Set(action, duration, tickset);
        }
        
        TimerHandle ICoreTask.Timer(float duration, bool countDown, ITicksetInstance tickset)
        {
            return new TimerHandle(_coreTick).Set(duration, countDown, tickset);
        }

        #endregion Tasks
    }
}