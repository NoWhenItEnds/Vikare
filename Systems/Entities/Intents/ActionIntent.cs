using Godot;

namespace Vikare.Entities.Intents
{
    /// <summary>
    /// Abstract base for all controller-to-state-machine messages.
    /// Both player and AI controllers produce subclasses; states consume them without caring about
    /// the source. Nearly every action has an associated facing component, so <see cref="Direction"/>
    /// is present on every intent.
    /// </summary>
    public abstract class ActionIntent
    {
        /// <summary>
        /// Desired movement or facing direction in world space; normalised or <see cref="Vector2.Zero"/>
        /// for no directional preference.
        /// </summary>
        public Vector2 Direction { get; }

        /// <summary>Initialises the base intent with a directional component.</summary>
        /// <param name="direction">Desired movement direction; normalised or <see cref="Vector2.Zero"/>.</param>
        protected ActionIntent(Vector2 direction)
        {
            Direction = direction;
        }
    }
}
