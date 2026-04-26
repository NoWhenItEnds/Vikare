using Godot;
using Vikare.Entities.Intents;

namespace Vikare.Entities.States
{
    /// <summary>
    /// Moves the actor at <see cref="Vikare.Entities.Entity.MaxSprintSpeed"/> in the direction supplied
    /// by the most-recent <see cref="SprintIntent"/>. Transitions are driven by the machine's transition table.
    /// </summary>
    public sealed class SprintingState : IState
    {
        /// <summary>Sprint animation clip name; must match the entity's animation library.</summary>
        private const string SprintAnimationName = "sprint";

        /// <summary>
        /// Most-recently received movement direction. Reset to zero on entry to prevent direction
        /// bleed from a prior activation; written by <see cref="HandleIntent"/>, read by
        /// <see cref="PhysicsProcess"/>.
        /// </summary>
        private Vector2 _currentDirection = Vector2.Zero;

        /// <summary>Resets direction to zero and plays the sprint animation.</summary>
        public void Enter(Actor actor)
        {
            _currentDirection = Vector2.Zero;
            actor.PlayAnimation(SprintAnimationName);
        }

        /// <inheritdoc/>
        public void Exit(Actor actor)
        {
        }

        /// <inheritdoc/>
        public void Process(Actor actor, double delta)
        {
        }

        /// <summary>Applies <see cref="_currentDirection"/> scaled by <see cref="Vikare.Entities.Entity.MaxSprintSpeed"/> each tick.</summary>
        public void PhysicsProcess(Actor actor, double delta)
        {
            actor.MovementVelocity = _currentDirection.Normalized() * actor.MaxSprintSpeed;
        }

        /// <summary>Updates <see cref="_currentDirection"/> from incoming <see cref="SprintIntent"/> messages.</summary>
        public void HandleIntent(Actor actor, ActionIntent intent)
        {
            if (intent is SprintIntent sprintIntent)
            {
                _currentDirection = sprintIntent.Direction;
            }
        }
    }
}
