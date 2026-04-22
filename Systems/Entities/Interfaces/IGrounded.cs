namespace Vikare.Entities.Interfaces
{
    /// <summary>
    /// Capability contract for any entity that distinguishes between being on the ground and being airborne.
    /// </summary>
    public interface IGrounded
    {
        /// <summary>
        /// Whether the entity is currently resting on a floor surface; updated after each <c>MoveAndSlide</c> call.
        /// </summary>
        bool IsOnGround { get; }
    }
}
