using System;
using GGSharpTick;

namespace GGSharpTask
{
    public class TimerHandle : TaskBase
    {
        #region Properties

        /// <summary>
        /// 
        /// </summary>
        public float NormalizedProgress => (float)Elapsed.TotalSeconds / _targetDuration;

        #endregion Properties


        #region Variables

        private Action<TimerHandle> _onUpdate;

        #endregion Variables


        #region Constructor

        internal TimerHandle(ICoreTick tick) : base(tick)
        {
            
        }

        #endregion Constructor


        #region Initialization

        /// <summary>
        /// 
        /// </summary>
        /// <param name="duration"></param>
        /// <param name="countDown"></param>
        /// <param name="tickset"></param>
        /// <returns></returns>
        public TimerHandle Set(
            float duration, 
            bool countDown,
            ITicksetInstance tickset)
        {
            _targetDuration = duration;
            SetTaskTick(tickset);

            return this;
        }

        #endregion Initialization


        #region Timer

        public new TimerHandle Begin()
        {
            base.Begin();
            return this;
        }

        public new TimerHandle LoopCount(int i)
        {
            base.LoopCount(i);
            return this;
        }

        #endregion Timer


        #region Callbacks

        public TimerHandle OnBegin(Action action)
        {
            base.SetOnBeginCallback(action);
            return this;
        }
        
        public TimerHandle OnUpdate(Action<TimerHandle> action)
        {
            _onUpdate = action;
            return this;
        }
        
        public TimerHandle OnLoop(Action<int> action)
        {
            base.SetOnLoopCallback(action);
            return this;
        }
        
        public TimerHandle OnComplete(Action action)
        {
            base.SetOnCompleteCallback(action);
            return this;
        }

        #endregion Callbacks


        #region Tick

        protected override void TaskTick(float delta)
        {
            _onUpdate?.Invoke(this);
        }

        protected override void OnTaskIterationEnd()
        {
            if (_targetLoops > 0)
            {
                _loopCount++;
                if (_loopCount < _targetLoops)
                {
                    base.TaskLooped();
                    return;
                }
            }
            
            base.Complete();
        }

        #endregion Tick


        #region Cleanup

        protected override void WipeTask()
        {
            _onUpdate = null;
            base.WipeTask();
        }

        #endregion Cleanup
        
        
        
        public bool TimerActive()
        {
            bool isFinished = Elapsed.TotalSeconds >= _targetDuration && _targetDuration > 0;
            return !isFinished;
        }
    }
}