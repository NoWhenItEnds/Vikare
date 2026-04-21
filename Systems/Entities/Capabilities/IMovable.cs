using Godot;

namespace Vikare.Entities.Capabilities
{
    /// <summary>
    /// Capability contract for any entity that can be moved by applying a velocity vector each physics tick.
    /// </summary>
    public interface IMovable
    {
        /// <summary>
        /// Velocity applied to the entity on the next physics tick; states write this, the entity reads it in <c>_PhysicsProcess</c>.
        /// </summary>
        Vector2 MovementVelocity { get; set; }

        /// <summary>
        /// Maximum travel speed in pixels per second for normal walking movement; must be positive.
        /// </summary>
        float MaxSpeed { get; }

        /// <summary>
        /// Maximum travel speed in pixels per second when sprinting; must exceed <see cref="MaxSpeed"/>.
        /// </summary>
        float MaxSprintSpeed { get; }

        /// <summary>
        /// Sets <see cref="MovementVelocity"/> to <see cref="Vector2.Zero"/>, exposing the stop as a method so callers can use the null-conditional operator.
        /// </summary>
        void ZeroVelocity();
    }
}
