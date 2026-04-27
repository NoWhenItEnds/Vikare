using System;
using Godot;

namespace Vikare.Entities.Abilities
{
    /// <summary>
    /// Shared base for melee hit effects; concrete subclasses supply the slot category.
    /// Assign a concrete subclass asset to <see cref="AbilityStep.Effect"/> for any melee-attack step.
    /// </summary>
    [GlobalClass]
    public abstract partial class MeleeHitAbilityEffect : AbilityEffect
    {
        /// <summary>
        /// Base damage dealt on a successful hit. Not yet wired to a damage system; author it into
        /// step assets now and connect it when the damage pipeline exists. Must be non-negative.
        /// </summary>
        [Export] public Single BaseDamage { get; set; } = 10f;


        /// <inheritdoc/>
        public override void Execute(Actor actor)
        {
            // STUB: replace with hitbox query and damage dispatch when the damage system exists.
            GD.Print($"[MeleeHitAbilityEffect] Melee hit frame fired. BaseDamage={BaseDamage}");
        }
    }
}
