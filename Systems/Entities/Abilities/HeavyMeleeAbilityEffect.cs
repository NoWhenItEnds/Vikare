using System;
using Godot;

namespace Vikare.Entities.Abilities
{
    /// <summary> Heavy melee hit effect — overhead strikes, heavy kicks. </summary>
    [GlobalClass]
    public partial class HeavyMeleeAbilityEffect : AbilityEffect
    {
        /// <summary> Base damage per hit. Not yet wired to the damage system; set in the asset now and connect later. Must be non-negative. </summary>
        [Export] public Single BaseDamage { get; set; } = 10f;

        /// <summary> Always <see cref="AbilityCategory.HeavyAttack"/>. </summary>
        public override AbilityCategory Category => AbilityCategory.HeavyAttack;

        /// <summary>Stub execute — replace with hitbox query and damage dispatch when the damage system exists.</summary>
        /// <param name="actor">The actor executing the ability step.</param>
        public override void Execute(Actor actor)
        {
            GD.Print($"[HeavyMeleeAbilityEffect] Melee hit frame fired. BaseDamage={BaseDamage}");
        }
    }
}
