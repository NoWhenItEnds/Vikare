using Godot;
using Vikare.Entities.Controllers;
using Vikare.Entities.Interfaces;

namespace Vikare.Entities.States
{
    /// <summary>
    /// Moves the entity at <see cref="IMovable.MaxSpeed"/> in the direction of the most-recent <see cref="MoveIntent"/>.
    /// </summary>
    public sealed class WalkingState : IState
    {
        /// <summary>
        /// Name of the walk animation clip; must match a clip in the entity's animation library.
        /// </summary>
        private const string WalkAnimationName = "walk";

        /// <summary>
        /// Most-recently received movement direction from the controller. Reset to zero on entry
        /// so re-entering the state never inherits a stale direction from a prior activation.
        /// </summary>
        private Vector2 _currentDirection = Vector2.Zero;

        /// <inheritdoc/>
        public void Enter(IStateContext context)
        {
            _currentDirection = Vector2.Zero;
            context.As<IAnimated>()?.PlayAnimation(WalkAnimationName);
        }

        /// <inheritdoc/>
        public void Exit(IStateContext context)
        {
        }

        /// <inheritdoc/>
        public void Process(IStateContext context, double delta)
        {
        }

        /// <inheritdoc/>
        public void PhysicsProcess(IStateContext context, double delta)
        {
            IMovable? movable = context.As<IMovable>();
            if (movable is not null)
            {
                movable.MovementVelocity = _currentDirection.Normalized() * movable.MaxSpeed;
            }
        }

        /// <inheritdoc/>
        /// <remarks>
        /// Updates <see cref="_currentDirection"/> from <see cref="MoveIntent"/>; all transition logic lives in the machine's transition table.
        /// </remarks>
        public void HandleIntent(IStateContext context, IInputIntent intent)
        {
            if (intent is MoveIntent moveIntent)
            {
                _currentDirection = moveIntent.Direction;
            }
        }
    }
}
