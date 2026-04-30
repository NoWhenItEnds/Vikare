using System;
using Godot;

namespace Vikare.Entities.Abilities
{
    /// <summary> An ability that results in a heavy melee attack. </summary>
    [GlobalClass]
    public partial class HeavyMeleeAbilityEffect : AbilityEffect
    {
        /// <summary> Base damage per hit. Not yet wired to the damage system; set in the asset now and connect later. Must be non-negative. </summary>
        [Export] public Single BaseDamage { get; set; } = 10f;


        /// <inheritdoc/>
        public override AbilityCategory Category => AbilityCategory.HeavyAttack;


        /// <inheritdoc/>
        public override void Execute(Actor actor)
        {
            GD.Print($"[HeavyMeleeAbilityEffect] Melee hit frame fired. BaseDamage={BaseDamage}");
        }
    }
}
