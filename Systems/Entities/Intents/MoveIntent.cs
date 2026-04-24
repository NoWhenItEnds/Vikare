using Godot;
using Vikare.Entities.Interfaces;

namespace Vikare.Entities.Intents
{
    /// <summary>
    /// An <see cref="IInputIntent"/> expressing the desired movement direction; a zero vector means "stop moving".
    /// </summary>
    public sealed class MoveIntent : IInputIntent
    {
        /// <summary>
        /// Desired movement direction in world space; expected to be normalised or <see cref="Vector2.Zero"/>.
        /// </summary>
        public Vector2 Direction { get; }

        /// <summary>
        /// Initialises a new <see cref="MoveIntent"/> with the specified direction.
        /// </summary>
        /// <param name="direction">The desired movement direction; pass <see cref="Vector2.Zero"/> to express an explicit stop.</param>
        public MoveIntent(Vector2 direction)
        {
            Direction = direction;
        }
    }
}
