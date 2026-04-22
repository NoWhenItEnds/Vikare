using Vikare.Entities.Interfaces;

namespace Vikare.Entities.Controllers
{
    /// <summary>
    /// An <see cref="IInputIntent"/> expressing whether the controller wants the entity to hold a blocking stance; edge-triggered on key press and release.
    /// </summary>
    public sealed class BlockIntent : IInputIntent
    {
        /// <summary>
        /// True when the block should begin; false when it should end.
        /// </summary>
        public bool IsBlocking { get; }

        /// <summary>
        /// Initialises a new <see cref="BlockIntent"/>.
        /// </summary>
        /// <param name="isBlocking">Pass <c>true</c> on block key press; <c>false</c> on block key release.</param>
        public BlockIntent(bool isBlocking)
        {
            IsBlocking = isBlocking;
        }
    }
}
