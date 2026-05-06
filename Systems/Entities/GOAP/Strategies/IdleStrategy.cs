using System;

namespace Vikare.Entities.GOAP.Strategies
{
    /// <summary> Stand around. Look pretty. </summary>
    public class IdleStrategy : IActionStrategy
    {
        /// <inheritdoc/>
        public Boolean IsValid { get; private set; } = true;    // We can always retardmax.

        /// <inheritdoc/>
        public Boolean IsComplete { get; private set; } = false;


        /// <summary> How long the action has remaining before it's complete. </summary>
        private Single _duration;

        /// <summary> The current amount of time remaining before the action is complete. </summary>
        private Double _currentTimeRemaining;


        /// <summary> A reference to the actor being manipulated. </summary>
        private readonly Actor _actor;


        /// <summary> Stand around. Look pretty. </summary>
        /// <param name="actor"> A reference to the actor being manipulated. </param>
        /// <param name="duration"> How long the action has remaining before it's complete. </param>
        public IdleStrategy(Actor actor, Single duration)
        {
            _actor = actor;   // TODO - This should play an idle animation.
            _duration = duration;
        }


        /// <inheritdoc/>
        public void Start()
        {
            _currentTimeRemaining = _duration;
        }


        /// <inheritdoc/>
        public void Update(Double delta)
        {
            _currentTimeRemaining -= delta;
            IsComplete = _currentTimeRemaining <= 0f;
        }


        /// <inheritdoc/>
        public void Stop() { }
    }
}
