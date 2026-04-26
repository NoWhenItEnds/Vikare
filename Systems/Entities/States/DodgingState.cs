using System;
using Godot;

namespace Vikare.Entities.States
{
    /// <summary> An entity wishes to avoid danger by not being somewhere. </summary>
    public sealed class DodgingState : IState
    {
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
        private Double _elapsed = 0.0;

        /// <summary>
        /// Resets the timer, snaps the dodge direction from the actor's current velocity, and plays
        /// the dodge animation.
        /// </summary>
        public void Enter(Actor actor)
        {
            _elapsed = 0.0;

            Vector2 currentVelocity = actor.Velocity;
            _dodgeDirection = currentVelocity != Vector2.Zero
                ? currentVelocity.Normalized()
                : Vector2.Zero;

            actor.PlayAnimation("dodge");
        }


        /// <inheritdoc/>
        public void Exit(Actor actor) { }


        /// <inheritdoc/>
        public void Process(Actor actor, Double delta) { }


        /// <inheritdoc/>
        public void PhysicsProcess(Actor actor, Double delta)
        {
            _elapsed += delta;

            Boolean shouldTransition = _dodgeDirection == Vector2.Zero;

            if (!shouldTransition)
            {
                actor.Velocity = _dodgeDirection * actor.MaxDodgeSpeed;
                shouldTransition = _elapsed >= actor.MaxDodgeDurationSeconds;
            }


            /// Self-transition is performed by calling <c>actor.Machine.ChangeState</c> directly rather than
            /// via a synthetic intent, because the elapsed timer fires from the physics loop, not from a
            /// controller event, and an intent that no controller ever produces would pollute the intent surface.
            if (shouldTransition)
            {
                actor.Machine.ChangeState<IdlingState>();
            }
        }


        /// <inheritdoc/>
        public void HandleIntent(Actor actor, ActionIntent intent) { }
    }
}
