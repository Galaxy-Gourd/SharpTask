using GGSharpPool;
using GGSharpTick;

namespace GGSharpTask
{
    public class TaskPool : PoolBase
    {
        #region Variables

        /// <summary>
        /// The core tick module to which we will subscribe newly created tasks
        /// </summary>
        private readonly ICoreTick _coreTick;

        #endregion Variables
        
        
        #region Construction

        public TaskPool(ICoreTick coreTick)
        {
            _coreTick = coreTick;
        }
        
        #endregion Construction
        
        
        #region Pooling

        protected override IClientPoolable CreateNewPoolable()
        {
            return new TaskHandle(_coreTick);
        }

        #endregion Pooling
        
    }
}