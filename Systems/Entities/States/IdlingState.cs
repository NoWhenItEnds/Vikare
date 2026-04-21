using Vikare.Entities.Capabilities;
using Vikare.Entities.Controllers;

namespace Vikare.Entities.States
{
    /// <summary>
    /// Resting state — zeros velocity, plays the idle animation, and waits for a <see cref="MoveIntent"/> to transition to <see cref="WalkingState"/>.
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
        public void HandleIntent(IStateContext context, IInputIntent intent)
        {
            if (intent is MoveIntent moveIntent)
            {
                bool wantsToMove = moveIntent.Direction != Godot.Vector2.Zero;
                IStateMachineAccess? machineAccess = context.As<IStateMachineAccess>();

                if (wantsToMove && machineAccess != null)
                {
                    machineAccess.Machine.ChangeState<WalkingState>();
                }
            }
        }
    }
}
