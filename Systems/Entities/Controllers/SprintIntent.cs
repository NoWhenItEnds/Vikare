namespace Vikare.Entities.Controllers
{
    /// <summary>
    /// An <see cref="IInputIntent"/> expressing whether the controller wants the entity to sprint; edge-triggered on key press and release.
    /// </summary>
    public sealed class SprintIntent : IInputIntent
    {
        /// <summary>
        /// True when the sprint should begin; false when it should end.
        /// </summary>
        public bool IsSprinting { get; }

        /// <summary>
        /// Initialises a new <see cref="SprintIntent"/>.
        /// </summary>
        /// <param name="isSprinting">Pass <c>true</c> on sprint key press; <c>false</c> on sprint key release.</param>
        public SprintIntent(bool isSprinting)
        {
            IsSprinting = isSprinting;
        }
    }
}
