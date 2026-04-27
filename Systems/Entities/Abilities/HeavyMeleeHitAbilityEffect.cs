using Godot;

namespace Vikare.Entities.Abilities
{
    /// <summary>
    /// Melee hit effect that slots into the heavy attack category (e.g. overhead strikes, heavy kicks).
    /// </summary>
    [GlobalClass]
    public partial class HeavyMeleeHitAbilityEffect : MeleeHitAbilityEffect
    {
        /// <inheritdoc/>
        public override AbilityCategory Category => AbilityCategory.HeavyAttack;
    }
}
