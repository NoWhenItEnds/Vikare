using System;
using Godot;

namespace Vikare.Entities.Abilities
{
    /// <summary>Melee hit effect for the light-attack slot — fast strikes, jabs.</summary>
    [GlobalClass]
    public partial class LightMeleeAbilityEffect : AbilityEffect
    {
        /// <summary>Base damage per hit. Not yet wired to the damage system; set in the asset now and connect later. Must be non-negative.</summary>
        [Export] public Single BaseDamage { get; set; } = 10f;

        /// <summary>Always <see cref="AbilityCategory.LightAttack"/>.</summary>
        public override AbilityCategory Category => AbilityCategory.LightAttack;

        /// <summary>Stub execute — replace with hitbox query and damage dispatch when the damage system exists.</summary>
        /// <param name="actor">The actor executing the ability step.</param>
        public override void Execute(Actor actor)
        {
            GD.Print($"[LightMeleeAbilityEffect] Melee hit frame fired. BaseDamage={BaseDamage}");
        }
    }
}
