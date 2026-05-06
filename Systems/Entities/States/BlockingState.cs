using System;
using Godot;

namespace Vikare.Entities.States
{
    /// <summary> An entity wishes to protect itself. </summary>
    public sealed class BlockingState : IState
    {
        /// <inheritdoc/>
        public String AnimationName => "block";


        /// <inheritdoc/>
        public void Enter(Actor actor)
        {
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
