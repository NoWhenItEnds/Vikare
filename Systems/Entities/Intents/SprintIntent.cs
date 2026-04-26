using Godot;

namespace Vikare.Entities.Intents
{
    /// <summary>
    /// Signals that the controller wants the entity to sprint in the given direction.
    /// Produced by <see cref="Vikare.Managers.InputManager"/> while the sprint input is held.
    /// </summary>
    /// <param name="direction">Desired movement direction; normalised or <see cref="Vector2.Zero"/>.</param>
    public class SprintIntent(Vector2 direction) : ActionIntent(direction) { }
}
