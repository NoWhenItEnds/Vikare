using Godot;
using Vikare.Entities.Intents;
using Vikare.Entities.Interfaces;

namespace Vikare.Entities.States
{
    /// <summary>
    /// Brief burst of movement in the direction snapped on entry, lasting
    /// <see cref="Vikare.Entities.Entity.MaxDodgeDurationSeconds"/> seconds.
    /// </summary>
    /// <remarks>
    /// If the actor was stationary when the dodge was triggered (<see cref="_dodgeDirection"/> is zero),
    /// the state immediately returns to <see cref="IdlingState"/> on the first physics tick.
    ///
    /// Self-transition is performed by calling <c>actor.Machine.ChangeState</c> directly rather than
    /// via a synthetic intent, because the elapsed timer fires from the physics loop — not from a
    /// controller event — and an intent that no controller ever produces would pollute the intent surface.
    /// </remarks>
    public sealed class DodgingState : IState
    {
        /// <summary>Dodge animation clip name; must match the entity's animation library.</summary>
        private const string DodgeAnimationName = "dodge";

        /// <summary>
        /// Direction snapped from the actor's current velocity on entry; held constant for the burst.
        /// Zero when the actor was stationary, causing an immediate idle transition.
        /// </summary>
        private Vector2 _dodgeDirection = Vector2.Zero;

        /// <summary>
        /// Seconds elapsed since the dodge began; compared against
        /// <see cref="Vikare.Entities.Entity.MaxDodgeDurationSeconds"/> each tick.
        /// Reset to zero on entry.
        /// </summary>
        private double _elapsed = 0.0;

        /// <summary>
        /// Resets the timer, snaps the dodge direction from the actor's current velocity, and plays
        /// the dodge animation.
        /// </summary>
        public void Enter(Actor actor)
        {
            _elapsed = 0.0;

            Vector2 currentVelocity = actor.MovementVelocity;
            _dodgeDirection = currentVelocity != Vector2.Zero
                ? currentVelocity.Normalized()
                : Vector2.Zero;

            actor.PlayAnimation(DodgeAnimationName);
        }

        /// <inheritdoc/>
        public void Exit(Actor actor)
        {
        }

        /// <inheritdoc/>
        public void Process(Actor actor, double delta)
        {
        }

        /// <summary>
        /// Writes dodge velocity each tick. Transitions to <see cref="IdlingState"/> when the actor
        /// was stationary on entry or the elapsed time exceeds the dodge duration.
        /// </summary>
        public void PhysicsProcess(Actor actor, double delta)
        {
            _elapsed += delta;

            bool shouldTransition = _dodgeDirection == Vector2.Zero;

            if (!shouldTransition)
            {
                actor.MovementVelocity = _dodgeDirection * actor.MaxDodgeSpeed;
                shouldTransition = _elapsed >= actor.MaxDodgeDurationSeconds;
            }

            if (shouldTransition)
            {
                actor.Machine.ChangeState<IdlingState>();
            }
        }

        /// <inheritdoc/>
        public void HandleIntent(Actor actor, ActionIntent intent)
        {
        }
    }
}
