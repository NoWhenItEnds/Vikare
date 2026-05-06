using System;
using Godot;

namespace Vikare.Entities.States
{
    /// <summary> Resting state. Sit around looking pretty. </summary>
    public sealed class IdlingState : IState
    {
        /// <inheritdoc/>
        public String AnimationName => "idle";


        /// <inheritdoc/>
        public void Enter(Actor actor)
        {
            // Zeroes velocity and plays the idle animation.
            actor.Velocity = Vector2.Zero;
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
