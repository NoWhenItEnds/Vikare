using System;
using Godot;
using Vikare.Entities.Components;

namespace Vikare.Entities.States
{
    /// <summary> An entity wishes to avoid danger by not being where it currently is. </summary>
    public sealed class DodgingState : IState
    {
        /// <summary> The absolute direction of the dodge. </summary>
        /// <remarks> A Vector.Zero indicates that a direction hasn't currently been selected. </remarks>
        private Vector2 _dodgeDirection = Vector2.Zero;

        /// <summary> The current time spent in the dodge state. The dodge state ends when this exceeds the target. </summary>
        private Single _currentTime = 0f;

        /// <summary> The total time that actor will spend in the dodge state. This is calculated on state Enter. </summary>
        private Single _targetTime = 1f;    // TODO - This should be calculated from entity weight.

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

            actor.PlayAnimation("dodge");
        }


        /// <inheritdoc/>
        public void Exit(Actor actor)
        {
            _dodgeDirection = Vector2.Zero;
            _currentTime = 0f;
            _targetTime = 1f;
            _speedModifier = 1f;
        }


        /// <inheritdoc/>
        public void Process(Actor actor, Double delta) { }


        /// <summary>Advances the dodge timer, writes velocity unconditionally, and transitions to idle once the burst completes or the actor was stationary on entry.</summary>
        public void PhysicsProcess(Actor actor, Double delta)
        {
            _currentTime += (Single)delta;
            actor.Velocity = _dodgeDirection.Normalized() * BASE_SPEED * _speedModifier;

            if (_currentTime >= _targetTime)
            {
                actor.Machine.ChangeState<IdlingState>();
            }
        }


        /// <inheritdoc/>
        public void HandleIntent(Actor actor, ActionIntent intent)
        {
            if (intent is DodgeIntent dodgeIntent)
            {
                // If our dodge is zero, it's the first frame, and we haven't chosen a direction.
                if (_dodgeDirection == Vector2.Zero)
                {
                    // Either dodge in the desired direction, or directly backwards.
                    _dodgeDirection = dodgeIntent.Direction != Vector2.Zero ?
                        dodgeIntent.Direction.Normalized() :
                        -actor.Direction;
                }
            }
        }
    }
}
