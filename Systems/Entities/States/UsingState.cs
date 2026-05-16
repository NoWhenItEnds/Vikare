using System;
using Godot;

namespace Vikare.Entities.States
{
    /// <summary> Active whilst the actor is performing a stationary interaction (e.g. resting, drinking). </summary>
    public sealed class UsingState : IState
    {
        /// <inheritdoc/>
        public String AnimationName => "use";


        /// <inheritdoc/>
        public void Enter(Actor actor)
        {
            // Zeroes velocity and plays the use animation.
            actor.DesiredVelocity = Vector2.Zero;
            actor.PlayAnimation(AnimationName, actor.Direction);
        }


        /// <inheritdoc/>
        public void Exit(Actor actor) { }


        /// <inheritdoc/>
        public void Process(Actor actor, Double delta) { }


        /// <inheritdoc/>
        public void PhysicsProcess(Actor actor, Double delta) { }


        /// <inheritdoc/>
        public void HandleIntent(Actor actor, ActionIntent intent) { }
    }
}
