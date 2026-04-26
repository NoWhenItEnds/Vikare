using Godot;

namespace Vikare.Entities.Intents
{
    /// <summary>
    /// Signals that the controller wants the entity to walk (or stand still) in the given direction.
    /// Produced every frame by <see cref="Vikare.Managers.InputManager"/> as the default intent.
    /// A <see cref="ActionIntent.Direction"/> of <see cref="Vector2.Zero"/> triggers the idle transition.
    /// </summary>
    /// <param name="direction">Desired movement direction; normalised or <see cref="Vector2.Zero"/>.</param>
    public class WalkIntent(Vector2 direction) : ActionIntent(direction) { }
}
