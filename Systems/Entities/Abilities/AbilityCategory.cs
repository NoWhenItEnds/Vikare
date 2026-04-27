namespace Vikare.Entities.Abilities
{
    /// <summary>
    /// Classifies an ability by its role in combat or movement. Also serves as the slot identity
    /// used by <see cref="Actor.GetAbility"/> — add a new value here and override
    /// <see cref="AbilityEffect.Category"/> in the new subclass; no other code changes are required (OCP).
    /// </summary>
    public enum AbilityCategory
    {
        /// <summary>Fast, low-commitment melee strike.</summary>
        LightAttack,

        /// <summary>Slow, high-commitment melee strike.</summary>
        HeavyAttack,

        /// <summary>Repositioning or evasion ability (e.g. teleport, dash).</summary>
        Movement,

        /// <summary>Ranged projectile attack.</summary>
        Projectile,

        /// <summary>Damage-mitigation or counter ability.</summary>
        Defensive,

        /// <summary>Miscellaneous ability that does not fit another category.</summary>
        Utility,
    }
}
