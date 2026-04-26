namespace Vikare.Entities.Intents
{
    /// <summary>
    /// Identifies which ability slot the controller wishes to activate or chain into.
    /// Used as the key in <see cref="Vikare.Entities.Combat.AbilityStep.Transitions"/> and
    /// <see cref="Vikare.Entities.Combat.AbilitySequence.EntrySteps"/>. Adding a new slot only
    /// requires a new enum member here; no state or machine code needs to change (OCP).
    /// </summary>
    public enum AbilityKey
    {
        /// <summary>Sentinel — a transition entry with this key is never valid.</summary>
        None = 0,

        /// <summary>Fast, low-damage melee strike. Mapped to <c>action_attack_light</c>.</summary>
        LightAttack = 1,

        /// <summary>Slow, high-damage melee strike. Mapped to <c>action_attack_heavy</c>.</summary>
        HeavyAttack = 2,

        /// <summary>Ability in power slot 1. Mapped to <c>action_power_00</c>.</summary>
        CastPower1 = 3,

        /// <summary>Ability in power slot 2. Mapped to <c>action_power_01</c>.</summary>
        CastPower2 = 4,

        /// <summary>Ability in power slot 3. Mapped to <c>action_power_02</c>.</summary>
        CastPower3 = 5,

        /// <summary>Ability in power slot 4. Mapped to <c>action_power_03</c>.</summary>
        CastPower4 = 6,
    }
}
