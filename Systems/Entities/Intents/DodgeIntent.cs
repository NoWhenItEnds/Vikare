using Godot;

namespace Vikare.Entities.Intents
{
    /// <summary>
    /// Signals that the controller wants the entity to execute a dodge roll.
    /// Produced by <see cref="Vikare.Managers.InputManager"/> on a just-pressed dodge input.
    /// The direction at the moment of the intent is used as the dodge travel direction.
    /// </summary>
    /// <param name="direction">Desired movement direction; normalised or <see cref="Vector2.Zero"/>.</param>
    public class DodgeIntent(Vector2 direction) : ActionIntent(direction) { }
}
