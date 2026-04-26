using System;
using Godot;

namespace Vikare.Entities.States
{
    /// <summary> An entity leisurely moves in a direction. </summary>
    public sealed class WalkingState : IState
    {
        /// <summary> Most-recently received movement direction. </summary>
        private Vector2 _currentDirection = Vector2.Zero;


        /// <inheritdoc/>
        public void Enter(Actor actor)
        {
            _currentDirection = Vector2.Zero;
            actor.PlayAnimation("walk");
        }


        /// <inheritdoc/>
        public void Exit(Actor actor) { }


        /// <inheritdoc/>
        public void Process(Actor actor, Double delta) { }


        /// <inheritdoc/>
        public void PhysicsProcess(Actor actor, Double delta)
        {
            actor.Velocity = _currentDirection.Normalized() * 200f; // TODO - Replace with value from MovementComponent.
        }


        /// <inheritdoc/>
        public void HandleIntent(Actor actor, ActionIntent intent)
        {
            if (intent is WalkIntent walkIntent)
            {
                _currentDirection = walkIntent.Direction;
            }
        }
    }
}
