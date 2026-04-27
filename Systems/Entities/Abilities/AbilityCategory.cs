namespace Vikare.Entities.Abilities
{
    /// <summary> Classifies an ability by its role in combat or movement. Also serves as the slot identity. </summary>
    public enum AbilityCategory
    {
        /// <summary> Fast, low-commitment melee strike. </summary>
        LightAttack,

        /// <summary> Slow, high-commitment melee strike. </summary>
        HeavyAttack,

        /// <summary> Repositioning or evasion ability (e.g. teleport, dash). </summary>
        Movement,

        /// <summary> Ranged projectile attack. </summary>
        Projectile,

        /// <summary> Damage-mitigation or counter ability. </summary>
        Defensive,

        /// <summary> Miscellaneous ability that does not fit another category. </summary>
        Utility,
    }
}
