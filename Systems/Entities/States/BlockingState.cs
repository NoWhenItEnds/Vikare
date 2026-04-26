namespace Vikare.Entities.States
{
    /// <summary>
    /// Stationary defensive stance — zeroes velocity and plays the block animation.
    /// Held while <see cref="BlockIntent.IsBlocking"/> is true; the machine's transition table returns
    /// to <see cref="IdlingState"/> on a <see cref="BlockIntent"/> with <c>IsBlocking == false</c>.
    /// </summary>
    public sealed class BlockingState : IState
    {
        /// <summary>Block animation clip name; must match the entity's animation library.</summary>
        private const string BlockAnimationName = "block";

        /// <summary>
        /// Zeroes velocity on entry so the actor cannot slide into the blocking stance if it was
        /// walking when the block input was pressed.
        /// </summary>
        public void Enter(Actor actor)
        {
            actor.ZeroVelocity();
            actor.PlayAnimation(BlockAnimationName);
        }

        /// <inheritdoc/>
        public void Exit(Actor actor)
        {
        }

        /// <inheritdoc/>
        public void Process(Actor actor, double delta)
        {
        }

        /// <inheritdoc/>
        public void PhysicsProcess(Actor actor, double delta)
        {
        }

        /// <inheritdoc/>
        public void HandleIntent(Actor actor, ActionIntent intent)
        {
        }
    }
}
