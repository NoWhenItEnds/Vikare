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
        /// <see cref="Actor.MaxDodgeDurationSeconds"/> each tick.
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


        /// <summary>Advances the dodge timer, writes velocity unconditionally, and transitions to idle once the burst completes or the actor was stationary on entry.</summary>
        public void PhysicsProcess(Actor actor, Double delta)
        {
            _elapsed += delta;
            actor.Velocity = _dodgeDirection * actor.MaxDodgeSpeed;

            Boolean shouldTransition = _dodgeDirection == Vector2.Zero
                || _elapsed >= actor.MaxDodgeDurationSeconds;

            // Transition via ChangeState directly; the elapsed timer fires from the physics loop,
            // not from a controller event, so a synthetic intent would pollute the intent surface.
            if (shouldTransition)
            {
                actor.Machine.ChangeState<IdlingState>();
            }
        }


        /// <summary>Ignored — direction is locked at <see cref="Enter"/> and held constant until the dodge completes.</summary>
        public void HandleIntent(Actor actor, ActionIntent intent) { }
    }
}
