using Godot;

namespace Vikare.Entities.Combat
{
    /// <summary>
    /// Placeholder for projectile-based fire attacks. Assign a <c>.tres</c> asset of this type to any
    /// step today; replace the <see cref="Execute"/> body with projectile spawn logic when that system
    /// exists — no other files need to change.
    /// </summary>
    [GlobalClass]
    public partial class FireballAbilityEffect : AbilityEffect
    {
        /// <summary>Stub implementation; prints a debug message until projectile spawning is implemented.</summary>
        /// <param name="actor">The actor executing the ability step.</param>
        public override void Execute(Actor actor)
        {
            // STUB: replace with projectile spawn logic when the projectile system exists.
            GD.Print("[FireballAbilityEffect] Projectile spawning is not yet implemented.");
        }
    }
}
