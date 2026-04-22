using Godot;
using Vikare.Entities.Controllers;
using Vikare.Entities.Interfaces;

namespace Vikare.Entities.States
{
    /// <summary>
    /// Brief burst of movement in the most-recently-known direction, lasting <see cref="IMovable.MaxDodgeDurationSeconds"/> seconds.
    /// On entry the current velocity direction is snapped; if the entity is stationary the velocity is zeroed and the machine
    /// returns to <see cref="IdlingState"/> on the next tick via <see cref="IStateMachineAccess.Machine"/>.
    /// Self-transition is performed directly through <see cref="IStateMachineAccess"/> rather than a synthetic intent because
    /// the elapsed timer fires from the physics loop — not from a controller event — and introducing an intent type that
    /// no controller ever produces would pollute the intent surface without adding clarity.
    /// Out of scope for this increment: invincibility frames, cooldown enforcement, directional input override mid-dodge.
    /// </summary>
    public sealed class DodgingState : IState
    {
        /// <summary>
        /// Name of the dodge animation clip; must match a clip in the entity's animation library.
        /// </summary>
        private const string DodgeAnimationName = "dodge";

        /// <summary>
        /// Direction the entity travels during the dodge, snapped from <see cref="IMovable.MovementVelocity"/> on entry.
        /// Remains <see cref="Vector2.Zero"/> when the entity was stationary, causing an immediate idle transition.
        /// </summary>
        private Vector2 _dodgeDirection = Vector2.Zero;

        /// <summary>
        /// Seconds elapsed since the dodge began; compared against <see cref="IMovable.MaxDodgeDurationSeconds"/> each physics tick.
        /// Reset to zero on entry so re-entering the state never inherits a stale timer.
        /// </summary>
        private double _elapsed = 0.0;

        /// <inheritdoc/>
        public void Enter(IStateContext context)
        {
            _elapsed = 0.0;

            IMovable? movable = context.As<IMovable>();
            Vector2 currentVelocity = movable?.MovementVelocity ?? Vector2.Zero;

            _dodgeDirection = currentVelocity != Vector2.Zero
                ? currentVelocity.Normalized()
                : Vector2.Zero;

            context.As<IAnimated>()?.PlayAnimation(DodgeAnimationName);
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
        /// Writes the dodge velocity each tick. When the entity was stationary on entry (<see cref="_dodgeDirection"/> is zero)
        /// or the elapsed time exceeds the duration, transitions immediately to <see cref="IdlingState"/> via
        /// <see cref="IStateMachineAccess.Machine"/>. The ChangeState call is deferred to the end of the method so velocity
        /// is fully written before the state is exited, honouring the single-exit constraint.
        /// </remarks>
        public void PhysicsProcess(IStateContext context, double delta)
        {
            _elapsed += delta;

            IMovable? movable = context.As<IMovable>();
            bool shouldTransition = _dodgeDirection == Vector2.Zero;

            if (!shouldTransition && movable is not null)
            {
                movable.MovementVelocity = _dodgeDirection * movable.MaxDodgeSpeed;
                shouldTransition = _elapsed >= movable.MaxDodgeDurationSeconds;
            }

            if (shouldTransition)
            {
                context.As<IStateMachineAccess>()?.Machine.ChangeState<IdlingState>();
            }
        }

        /// <inheritdoc/>
        /// <remarks>
        /// Dodge direction is snapped on entry and cannot be steered; all transition logic lives in the machine's transition table
        /// or the duration timer above.
        /// </remarks>
        public void HandleIntent(IStateContext context, IInputIntent intent)
        {
        }
    }
}
