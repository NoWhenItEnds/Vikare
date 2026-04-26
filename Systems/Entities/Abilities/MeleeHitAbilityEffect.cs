using Godot;

namespace Vikare.Entities.Abilities
{
    /// <summary>
    /// Represents a physical melee hit landing at <see cref="AbilityStep.EffectFrameSeconds"/>.
    /// Assign to the <see cref="AbilityStep.Effect"/> slot of any melee-attack step.
    /// </summary>
    [GlobalClass]
    public partial class MeleeHitAbilityEffect : AbilityEffect
    {
        /// <summary>
        /// Base damage dealt on a successful hit. Not yet wired to a damage system; author it into
        /// step assets now and connect it when the damage pipeline exists. Must be non-negative.
        /// </summary>
        [Export] public float BaseDamage { get; set; } = 10f;

        /// <summary>Stub implementation; prints a debug message until the damage system exists.</summary>
        /// <param name="actor">The actor executing the hit.</param>
        public override void Execute(Actor actor)
        {
            // STUB: replace with hitbox query and damage dispatch when the damage system exists.
            GD.Print($"[MeleeHitAbilityEffect] Melee hit frame fired. BaseDamage={BaseDamage}");
        }
    }
}
