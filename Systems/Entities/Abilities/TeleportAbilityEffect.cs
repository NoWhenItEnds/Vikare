using Godot;

namespace Vikare.Entities.Abilities
{
    /// <summary>
    /// Instantly displaces the actor by a fixed world-space offset, creating a blink-teleport effect.
    /// Assign to <see cref="AbilityStep.Effect"/> on any step that should produce a position jump.
    /// </summary>
    [GlobalClass]
    public partial class TeleportAbilityEffect : AbilityEffect
    {
        /// <inheritdoc/>
        public override AbilityCategory Category => AbilityCategory.Movement;

        /// <summary>
        /// World-space displacement in pixels added to the actor's position when the effect fires.
        /// Positive X moves right; positive Y moves down (Godot 2D convention).
        /// </summary>
        [Export] public Vector2 Offset { get; set; } = Vector2.Zero;


        /// <inheritdoc/>
        public override void Execute(Actor actor)
        {
            actor.GlobalPosition += Offset;
        }
    }
}
