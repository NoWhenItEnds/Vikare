using System;
using Godot;
using Vikare.Entities.Components;

namespace Vikare.Entities.States
{
    /// <summary> An entity energetically moves in a direction. </summary>
    public sealed class SprintingState : IState
    {
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
                // Average of strength + finesse.
                _speedModifier = (attributeComponent.Strength + attributeComponent.Finesse) * 0.5f;
            }

            actor.PlayAnimation("sprint");
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
        public void PhysicsProcess(Actor actor, double delta)
        {
            actor.Velocity = _currentDirection.Normalized() * BASE_SPEED * _speedModifier;
        }


        /// <inheritdoc/>
        public void HandleIntent(Actor actor, ActionIntent intent)
        {
            if (intent is SprintIntent sprintIntent)
            {
                _currentDirection = sprintIntent.Direction;
            }
        }
    }
}
