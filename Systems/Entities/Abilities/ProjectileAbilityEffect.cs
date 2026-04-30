using Godot;

namespace Vikare.Entities.Abilities
{
    /// <summary> A projectile launched in a direction from a source. </summary>
    [GlobalClass]
    public partial class ProjectileAbilityEffect : AbilityEffect
    {
        /// <inheritdoc/>
        public override AbilityCategory Category => AbilityCategory.Projectile;

        /// <inheritdoc/>
        public override void Execute(Actor actor)
        {
            // STUB: replace with projectile spawn logic when the projectile system exists.
            GD.Print("[ProjectileAbilityEffect] Projectile spawning is not yet implemented.");
        }
    }
}
