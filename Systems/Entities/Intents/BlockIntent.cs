using Godot;

namespace Vikare.Entities.Intents
{
    /// <summary>
    /// Signals that the controller wants the entity to enter or hold a blocking stance.
    /// <see cref="IsBlocking"/> drives both the enter transition (true) and the exit transition (false),
    /// allowing a single intent type to communicate a held-action start and release.
    /// </summary>
    public class BlockIntent : ActionIntent
    {
        /// <summary>True while the block input is held; false when released.</summary>
        public bool IsBlocking { get; }

        /// <summary>Initialises a <see cref="BlockIntent"/> with direction and blocking flag.</summary>
        /// <param name="direction">Desired movement direction; normalised or <see cref="Vector2.Zero"/>.</param>
        /// <param name="isBlocking">True when actively holding the block input; false on release.</param>
        public BlockIntent(Vector2 direction, bool isBlocking) : base(direction)
        {
            IsBlocking = isBlocking;
        }
    }
}
