using System;
using System.Diagnostics;
using GGSharpPool;
using GGSharpTick;

namespace GGSharpTask
{
    public abstract class TaskBase : ITickClientFixed, ITickClientVariable, IClientPoolable
    {
        #region Properties

        /// <summary>
        /// Direct access to normalized task progress
        /// </summary>
        public float Progress { get; protected set; }

        /// <summary>
        /// Direct access to raw task duration
        /// </summary>
        public TimeSpan Elapsed => _stopwatch.Elapsed;
        
        /// <summary>
        /// Direct access to raw task duration
        /// </summary>
        public TimeSpan LoopElapsed => _stopwatch.Elapsed - _previousLoopElapsedCount;
        
        /// <summary>
        /// The target duration of this task (in seconds)
        /// </summary>
        protected float _targetDuration { get; set; }
        
        /// <summary>
        /// Used for tracking time
        /// </summary>
        private readonly Stopwatch _stopwatch = new Stopwatch();

        public bool AvailableInPool
        {
            get => _currentOwnershipState == TaskOwnershipState.Available;
            set {  }
        }

        #endregion Properties
        
        
        #region Variables
        
        /// <summary>
        /// The task's current operating state.
        /// </summary>
        protected TaskOwnershipState _currentOwnershipState { get; private set; }
        protected enum TaskOwnershipState
        {
            Available,
            Claimed
        }

        protected int _loopCount;
        protected int _targetLoops;
        protected bool _paused;
        private TimeSpan _previousLoopElapsedCount;
        
        private Action _onBegin;
        private Action<int> _onLoop;
        private Action _onComplete;

        /// <summary>
        /// 
        /// </summary>
        private readonly ICoreTick _coreTick;
        
        /// <summary>
        /// 
        /// </summary>
        private ITicksetInstance _tick;
        
        /// <summary>
        /// 
        /// </summary>
        private PoolBase _pool;
        
        #endregion Variables


        #region Construction

        public TaskBase(ICoreTick tick)
        {
            _coreTick = tick;
        }

        #endregion Construction

        
        #region Tickset

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        protected void SetTaskTick(ITicksetInstance data)
        {
            if (_tick == null)
            {
                _coreTick.Register(this, data);
            }
            else if (_tick != data)
            {
                _coreTick.Unregister(this, data);
                _coreTick.Register(this, data);
            }
            
            _tick = data;
        }

        #endregion Tickset


        #region Operation Lifetime

        /// <summary>
        /// Begins execution of a task.
        /// </summary>
        protected void Begin()
        {
            _onBegin?.Invoke();
            _stopwatch.Start();
        }
        
        /// <summary>
        /// Pauses execution of a task without clearing callbacks.
        /// </summary>
        protected void Pause()
        {
            _stopwatch.Stop();
            _paused = true;
        }
        
        /// <summary>
        /// Resumes execution of a task.
        /// </summary>
        protected void Resume()
        {
            _stopwatch.Start();
            _paused = false;
        }
        
        /// <summary>
        /// Restarts execution of a task from the beginning.
        /// </summary>
        protected void Restart()
        {
            _stopwatch.Stop();
            Begin();
        }
        
        /// <summary>
        /// Cancels execution of a task; clears callbacks.
        /// </summary>
        protected void Cancel()
        {
            EndTask();
        }
        
        /// <summary>
        /// When this task finishes a loop iteration.
        /// </summary>
        protected void TaskLooped()
        {
            Progress = 0;
            _previousLoopElapsedCount += LoopElapsed;
            _onLoop?.Invoke(_loopCount);
        }
        
        /// <summary>
        /// Forces completion of a task; clears callbacks.
        /// </summary>
        protected void Complete()
        {
            _onComplete?.Invoke();
            EndTask();
        }
        
        /// <summary>
        /// Ends and relinquishes task instance.
        /// </summary>
        private void EndTask()
        {
            _stopwatch.Stop();
            _pool?.RelinquishInstance(this);
        }

        #endregion Operation Lifetime


        #region Callbacks

        protected void SetOnBeginCallback(Action action)
        {
            _onBegin = action;
        }
        
        protected void SetOnLoopCallback(Action<int> action)
        {
            _onLoop = action;
        }
        
        protected void SetOnCompleteCallback(Action action)
        {
            _onComplete = action;
        }

        #endregion Callbacks
        

        #region Modifiers

        protected void LoopCount(int i)
        {
            _targetLoops = i;
        }

        #endregion Modifiers
        
        
        #region Tick
        
        void ITickClientFixed.Tick(float delta)
        {
            TaskTickBase(delta);
        }

        void ITickClientVariable.Tick(float delta)
        {
            TaskTickBase(delta);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="delta"></param>
        private void TaskTickBase(float delta)
        {
            if (_currentOwnershipState == TaskOwnershipState.Claimed && !_paused)
            {
                // Set normalized progress
                if (_targetDuration > 0)
                {
                    Progress = (float) LoopElapsed.TotalSeconds / _targetDuration;
                    if (Progress > 1)
                    {
                        OnTaskIterationEnd();
                        return;
                    }
                }
                
                // Tick
                TaskTick(delta);
            }
        }

        /// <summary>
        /// Ticks the task after verifing the task is active and claimed.
        /// </summary>
        /// <param name="delta"></param>
        protected abstract void TaskTick(float delta);

        /// <summary>
        /// 
        /// </summary>
        protected abstract void OnTaskIterationEnd();

        #endregion Tick
        
        
        #region Pooling

        void IClientPoolable.OnInstanceCreated(PoolBase pool)
        {
            _pool = pool;
        }

        void IClientPoolable.Claim()
        {
            _currentOwnershipState = TaskOwnershipState.Claimed;
        }
        
        void IClientPoolable.Relinquish()
        {
            _currentOwnershipState = TaskOwnershipState.Available;
            WipeTask();
        }
        
        void IClientPoolable.Recycle()
        {
            WipeTask();
        }
        
        void IClientPoolable.DeleteFromPool()
        {
            _coreTick.Unregister(this, _tick);
        }

        #endregion Pooling


        #region Utility

        /// <summary>
        /// Resets properties for future use when task is relinquished.
        /// </summary>
        protected virtual void WipeTask()
        {
            // Flush properties
            _loopCount = 0;
            _targetLoops = 0;
            _paused = false;
            Progress = 0;
            _targetDuration = 0;
            _stopwatch.Reset();
            _pool = null;
            _previousLoopElapsedCount = TimeSpan.Zero;

            // Flush callbacks
            _onBegin = null;
            _onLoop = null;
            _onComplete = null;
        }

        #endregion Utility
    }
}