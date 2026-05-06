using System;
using Godot;
using Vikare.Entities.Components;

namespace Vikare.Entities.States
{
    /// <summary> An entity leisurely moves in a direction. </summary>
    public sealed class WalkingState : IState
    {
        /// <inheritdoc/>
        public String AnimationName => "walk";


        /// <summary> Most-recently received movement direction. </summary>
        private Vector2 _currentDirection = Vector2.Zero;

        /// <summary> The current modifier applied to base speed. </summary>
        private Single _speedModifier = 1f;

        /// <summary> The basic, unmodified movement speed. </summary>
        private const Single BASE_SPEED = 100f;


        /// <inheritdoc/>
        public void Enter(Actor actor)
        {
            AttributeComponent? attributeComponent = actor.GetComponent<AttributeComponent>();
            if (attributeComponent != null)
            {
                // 10% of average of strength + finesse.
                _speedModifier = 1f + (attributeComponent.Strength.CurrentValue + attributeComponent.Finesse.CurrentValue) * 0.05f;
            }
        }


        /// <inheritdoc/>
        public void Exit(Actor actor)
        {
            _currentDirection = Vector2.Zero;
            _speedModifier = 1f;
        }


        /// <inheritdoc/>
        public void Process(Actor actor, Double delta) { }


        /// <inheritdoc/>
        public void PhysicsProcess(Actor actor, Double delta)
        {
            actor.Velocity = _currentDirection.Normalized() * BASE_SPEED * _speedModifier;
        }


        /// <inheritdoc/>
        public void HandleIntent(Actor actor, ActionIntent intent)
        {
            if (intent is WalkIntent walkIntent)
            {
                _currentDirection = walkIntent.Direction;
                actor.PlayAnimation(AnimationName, _currentDirection);
            }
        }
    }
}
