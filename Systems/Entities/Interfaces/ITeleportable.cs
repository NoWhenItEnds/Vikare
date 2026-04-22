using Godot;

namespace Vikare.Entities.Interfaces
{
    /// <summary>
    /// Capability contract for any entity whose world position can be set directly,
    /// bypassing normal movement and collision resolution.
    /// Lives on <see cref="Vikare.Entities.Entity"/> rather than <see cref="Vikare.Entities.Actor"/>
    /// because positional control is meaningful for any CharacterBody2D node — props, projectiles,
    /// or hazards may also need to be repositioned by scripted events without being full actors.
    /// <c>CharacterBody2D</c> already exposes <c>GlobalPosition</c>, so the implementation in
    /// <see cref="Vikare.Entities.Entity"/> is a one-line trivial property delegation.
    /// </summary>
    public interface ITeleportable
    {
        /// <summary>
        /// World-space position of the entity in pixels; set directly to teleport without physics resolution.
        /// Read to query the entity's current world position.
        /// </summary>
        Vector2 GlobalPosition { get; set; }
    }
}
