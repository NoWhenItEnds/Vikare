using Godot;
using Vikare.Entities.Interfaces;

namespace Vikare.Entities.Combat
{
    /// <summary>
    /// A <see cref="PowerEffect"/> that instantly displaces the entity by a fixed world-space offset.
    /// Queries <see cref="ITeleportable"/> from the context; silently does nothing if the entity does not implement it.
    /// Wire an instance of this resource to <see cref="PowerDefinition.Effect"/> on a teleport power asset.
    /// </summary>
    [GlobalClass]
    public partial class TeleportPowerEffect : PowerEffect
    {
        /// <summary>
        /// World-space displacement in pixels applied to the entity's <see cref="ITeleportable.GlobalPosition"/> when the effect fires.
        /// Positive X moves right; positive Y moves down (Godot's 2D convention).
        /// Set in the editor per power asset — e.g. <c>(200, 0)</c> for a rightward dash-teleport.
        /// </summary>
        [Export] public Vector2 Offset { get; set; } = Vector2.Zero;

        /// <summary>
        /// Adds <see cref="Offset"/> to the entity's <see cref="ITeleportable.GlobalPosition"/>.
        /// No-ops silently if the entity does not implement <see cref="ITeleportable"/>.
        /// </summary>
        /// <param name="context">Entity context; must expose <see cref="ITeleportable"/> for the effect to apply.</param>
        public override void Execute(IStateContext context)
        {
            ITeleportable? teleportable = context.As<ITeleportable>();
            if (teleportable is not null)
            {
                teleportable.GlobalPosition += Offset;
            }
        }
    }
}
