using System;
using System.Collections.Generic;
using GGSharpTick;

namespace GGSharpTask
{
    public class TaskHandle : TaskBase
    {
        #region Variables

        public enum TaskLoopMode
        {
            None,
            Loop,
            PingPong
        }
        private TaskLoopMode _loopMode;

        /// <summary>
        /// 
        /// </summary>
        private Action<float> _taskAction;

        /// <summary>
        /// 
        /// </summary>
        private readonly List<Func<bool>> _taskConditions = new List<Func<bool>>();
        
        //
        private ConditionsEvalOptions _evaluationMode;
        private ConditionsFailResultOptions _conditionsFailMode;
        
        #endregion Variables


        #region Properties

        /// <summary>
        /// If true, the task is currently in the reverse phase of a loop.
        /// </summary>
        public bool ReverseFlag { get; private set; }

        #endregion Properties
        
        
        #region Constructor

        public TaskHandle(ICoreTick tick) : base(tick)
        {
            
        }

        #endregion Constructor


        #region Initialization

        /// <summary>
        /// 
        /// </summary>
        /// <param name="action"></param>
        /// <param name="duration"></param>
        /// <param name="tickset"></param>
        /// <returns></returns>
        internal TaskHandle Set(
            Action<float> action, 
            float duration, 
            ITicksetInstance tickset)
        {
            _taskAction = action;
            _targetDuration = duration;
            if (duration > 0)
            {
                AddCondition(duration);
            }
            
            // Set default tickset
            SetTaskTick(tickset);
            return this;
        }

        /// <summary>
        /// Assigns a new tickset to the task
        /// </summary>
        /// <param name="tickset">The tickset to assign this task to.</param>
        /// <returns>For method chaining.</returns>
        internal TaskHandle AssignTickset(ITicksetInstance tickset)
        {
            SetTaskTick(tickset);
            return this;
        }

        #endregion Initialization
        
        
        #region Task
        
        public new TaskHandle Begin()
        {
            base.Begin();
            return this;
        }

        public new TaskHandle Cancel()
        {
            base.Cancel();
            return this;
        }
        
        public new TaskHandle Pause()
        {
            base.Pause();
            return this;
        }
        
        public new TaskHandle Resume()
        {
            base.Resume();
            return this;
        }
        
        public new TaskHandle Restart()
        {
            base.Restart();
            return this;
        }
        
        public new TaskHandle Complete()
        {
            base.Complete();
            return this;
        }

        #endregion Task


        #region Modifiers

        /// <summary>
        /// 
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public new TaskHandle LoopCount(int i)
        {
            base.LoopCount(i);
            if (_loopMode == TaskLoopMode.None)
            {
                _loopMode = TaskLoopMode.Loop;
            }
            return this;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        public TaskHandle LoopMode(TaskLoopMode m)
        {
            if (_loopMode == TaskLoopMode.None && m != TaskLoopMode.None)
            {
                LoopCount(-1);
            }
            _loopMode = m;
            return this;
        }

        #endregion Modifiers
        
        
        #region Callbacks

        public TaskHandle OnBegin(Action action)
        {
            base.SetOnBeginCallback(action);
            return this;
        }
        
        public TaskHandle OnLoop(Action<int> action)
        {
            base.SetOnLoopCallback(action);
            return this;
        }
        
        public TaskHandle OnComplete(Action action)
        {
            base.SetOnCompleteCallback(action);
            return this;
        }

        #endregion Callbacks


        #region Conditions

        /// <summary>
        /// Adds a condition to the list of requisite task conditions.
        /// </summary>
        /// <param name="condition">The new condition.</param>
        /// <returns>Returns itself for method chaining.</returns>
        public TaskHandle AddCondition(Func<bool> condition)
        {
            _taskConditions.Add(condition);
            return this;
        }

        /// <summary>
        /// Adds a timer condition to the list of task conditions.
        /// </summary>
        /// <param name="duration"></param>
        private void AddCondition(float duration)
        {
            //TimerHandle t = Core.Timer.Set(duration);
            //_taskConditions.Add(t.TimerActive);
        }

        /// <summary>
        /// Removes a condition from the list of conditions
        /// </summary>
        /// <param name="condition"></param>
        /// <returns></returns>
        public TaskHandle RemoveCondition(Func<bool> condition)
        {
            _taskConditions.Remove(condition);
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mode"></param>
        /// <returns></returns>
        public TaskHandle EvaluationMode(ConditionsEvalOptions mode)
        {
            _evaluationMode = mode;
            return this;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="mode"></param>
        /// <returns></returns>
        public TaskHandle FailMode(ConditionsFailResultOptions mode)
        {
            _conditionsFailMode = mode;
            return this;
        }

        #endregion Conditions
        
        
        #region Tick

        protected override void TaskTick(float delta)
        {
            // Check our conditions
            if (TaskConditionEvaluator.EvaluateTaskConditions(_evaluationMode, _taskConditions))
            {
                // If our task was paused to wait for condition success, resume
                if (_paused)
                {
                    Resume();
                }
                
                TaskProgressScaler(delta);
                ExecuteTaskAction();
            }
            else
            {
                switch (_conditionsFailMode)
                {
                    case ConditionsFailResultOptions.End:
                        Cancel();
                        break;
                    case ConditionsFailResultOptions.Pause:
                        Pause();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        protected override void OnTaskIterationEnd()
        {
            // Do we need to loop?
            if (_loopMode != TaskLoopMode.None)
            {
                if (_targetLoops <= 0 || _loopCount + 1 < _targetLoops)
                {
                    _loopCount++;
                    switch (_loopMode)
                    {
                        case TaskLoopMode.Loop:
                        case TaskLoopMode.None:
                            ReverseFlag = false;
                            break;
                        case TaskLoopMode.PingPong:
                            ReverseFlag = !ReverseFlag;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    base.TaskLooped();
                    return;
                }
            }

            Complete();
        }
        
        /// <summary>
        /// Scales the progress of the task.
        /// </summary>
        /// <param name="delta">Time in seconds elapsed since previous task tick.</param>
        protected virtual void TaskProgressScaler(float delta)
        {
            if (ReverseFlag) 
            { 
                Progress = 1 - Progress;
            }
        }

        /// <summary>
        /// Calls the action assigned to this task.
        /// </summary>
        protected virtual void ExecuteTaskAction()
        {
            _taskAction?.Invoke(Progress);
        }
        
        #endregion


        #region Cleanup

        protected override void WipeTask()
        {
            ReverseFlag = false;
            _loopMode = TaskLoopMode.None;
            _taskConditions.Clear();
            _taskAction = null;
            _evaluationMode = ConditionsEvalOptions.All;
            base.WipeTask();
        }

        #endregion Cleanup
    }
}