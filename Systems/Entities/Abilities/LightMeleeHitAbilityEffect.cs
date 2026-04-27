using Godot;

namespace Vikare.Entities.Abilities
{
    /// <summary>
    /// Melee hit effect that slots into the light attack category (e.g. jabs, fast strikes).
    /// </summary>
    [GlobalClass]
    public partial class LightMeleeHitAbilityEffect : MeleeHitAbilityEffect
    {
        /// <inheritdoc/>
        public override AbilityCategory Category => AbilityCategory.LightAttack;
    }
}
