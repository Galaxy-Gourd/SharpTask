using System;
using GGSharpTick;

namespace GGSharpTask
{
    public interface ICoreTask
    {
        #region Methods

        /// <summary>
        /// Returns a task for immediate use.
        /// </summary>
        /// <param name="action">The task handle that will receive tick callbacks from the task.</param>
        /// <param name="tickset">The tickset to which the task should be assigned</param>
        /// <returns></returns>
        TaskHandle Task(Action<float> action, ITicksetInstance tickset = null);
        
        // /// <summary>
        // /// Returns a timed task for immediate use.
        // /// </summary>
        // /// <param name="action">The task handle that will receive tick callbacks from the task.</param>
        // /// <param name="duration">The duration of the timed task.</param>
        // /// <param name="tickset">The tickset to which the task should be assigned</param>
        // /// <returns></returns>
        TaskHandle Task(Action<float> action, float duration, ITicksetInstance tickset = null);
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="duration"></param>
        /// <param name="countDown"></param>
        /// <param name="tickset"></param>
        /// <returns></returns>
        TimerHandle Timer(float duration, bool countDown = false, ITicksetInstance tickset = null);

        #endregion Methods
    }
}