using Vikare.Entities.Intents;
using Vikare.Entities.Interfaces;

namespace Vikare.Entities.States
{
    /// <summary>
    /// Resting state — zeros velocity and plays the idle animation. Transitions are driven by the machine's transition table.
    /// </summary>
    public sealed class IdlingState : IState
    {
        /// <summary>
        /// Name of the idle animation clip; must match a clip in the entity's animation library.
        /// </summary>
        private const string IdleAnimationName = "idle";

        /// <inheritdoc/>
        public void Enter(IStateContext context)
        {
            context.As<IMovable>()?.ZeroVelocity();
            context.As<IAnimated>()?.PlayAnimation(IdleAnimationName);
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
        public void PhysicsProcess(IStateContext context, double delta)
        {
        }

        /// <inheritdoc/>
        /// <remarks>
        /// Idle holds no per-intent data; all transition logic lives in the machine's transition table.
        /// </remarks>
        public void HandleIntent(IStateContext context, IInputIntent intent)
        {
        }
    }
}
