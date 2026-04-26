using Godot;
using Vikare.Entities.Intents;

namespace Vikare.Entities.States
{
    /// <summary>
    /// Moves the actor at <see cref="Vikare.Entities.Entity.MaxSpeed"/> in the direction supplied by
    /// the most-recent <see cref="WalkIntent"/>. Transitions are driven by the machine's transition table.
    /// </summary>
    public sealed class WalkingState : IState
    {
        /// <summary>Walk animation clip name; must match the entity's animation library.</summary>
        private const string WalkAnimationName = "walk";

        /// <summary>
        /// Most-recently received movement direction. Reset to zero on entry to prevent direction
        /// bleed from a prior activation; written by <see cref="HandleIntent"/>, read by
        /// <see cref="PhysicsProcess"/>.
        /// </summary>
        private Vector2 _currentDirection = Vector2.Zero;

        /// <summary>Resets direction to zero and plays the walk animation.</summary>
        public void Enter(Actor actor)
        {
            _currentDirection = Vector2.Zero;
            actor.PlayAnimation(WalkAnimationName);
        }

        /// <inheritdoc/>
        public void Exit(Actor actor)
        {
        }

        /// <inheritdoc/>
        public void Process(Actor actor, double delta)
        {
        }

        /// <summary>Applies <see cref="_currentDirection"/> scaled by <see cref="Vikare.Entities.Entity.MaxSpeed"/> each tick.</summary>
        public void PhysicsProcess(Actor actor, double delta)
        {
            actor.MovementVelocity = _currentDirection.Normalized() * actor.MaxSpeed;
        }

        /// <summary>Updates <see cref="_currentDirection"/> from incoming <see cref="WalkIntent"/> messages.</summary>
        public void HandleIntent(Actor actor, ActionIntent intent)
        {
            if (intent is WalkIntent walkIntent)
            {
                _currentDirection = walkIntent.Direction;
            }
        }
    }
}
