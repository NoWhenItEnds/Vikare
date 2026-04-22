using Vikare.Entities.Interfaces;

namespace Vikare.Entities.States
{
    /// <summary>
    /// Stationary defensive stance — zeros velocity and plays the block animation.
    /// Held for as long as the controller sends <see cref="Controllers.BlockIntent"/> with <c>IsBlocking == true</c>;
    /// the machine's transition table returns to <see cref="IdlingState"/> on release.
    /// Out of scope for this increment: damage reduction, parry windows, stamina cost.
    /// </summary>
    public sealed class BlockingState : IState
    {
        /// <summary>
        /// Name of the block animation clip; must match a clip in the entity's animation library.
        /// </summary>
        private const string BlockAnimationName = "block";

        /// <inheritdoc/>
        public void Enter(IStateContext context)
        {
            context.As<IMovable>()?.ZeroVelocity();
            context.As<IAnimated>()?.PlayAnimation(BlockAnimationName);
        }

        /// <inheritdoc/>
        public void Exit(IStateContext context)
        {
        }

        /// <inheritdoc/>
        public void Process(IStateContext context, double delta)
        {
        }

        /// <inheritdoc/>
        /// <remarks>
        /// Velocity was zeroed on entry; no further update is needed because no movement intent is consumed while blocking.
        /// </remarks>
        public void PhysicsProcess(IStateContext context, double delta)
        {
        }

        /// <inheritdoc/>
        /// <remarks>
        /// Blocking holds no per-intent data; all transition logic lives in the machine's transition table.
        /// </remarks>
        public void HandleIntent(IStateContext context, IInputIntent intent)
        {
        }
    }
}
