using System;

namespace Vikare.Entities.GOAP.Strategies
{
    /// <summary> The logic that is used to evaluate / carry out an action. </summary>
    public interface IActionStrategy
    {
        /// <summary> Whether the strategy is currently valid. </summary>
        public Boolean IsValid { get; }

        /// <summary> If the action has been completed. </summary>
        public Boolean IsComplete { get; }


        /// <summary> Called to initialise / start the action. </summary>
        public void Start();

        /// <summary> Called every 'frame' the action is being processed. </summary>
        /// <param name="delta"> The time since the action was last updated. </param>
        public void Update(Double delta);


        /// <summary> Called to cancelled or stop the action. </summary>
        public void Stop();
    }
}
