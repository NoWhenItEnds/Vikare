using Godot;

namespace Vikare.Entities.Intents
{
    /// <summary>
    /// Unified intent for any ability activation — melee attacks, power casts, or future slots.
    /// A single type is used because the ability graph allows any step to chain into any other step
    /// regardless of type; the <see cref="Key"/> is the shared vocabulary between the transition table
    /// and the active step's transition map.
    /// </summary>
    public class AbilityIntent : ActionIntent
    {
        /// <summary>
        /// Which ability slot this intent activates; must not be <see cref="AbilityKey.None"/>.
        /// Used to look up the entry step from <see cref="Vikare.Entities.Combat.AbilitySequence.EntrySteps"/>
        /// or a chain step from <see cref="Vikare.Entities.Combat.AbilityStep.Transitions"/>.
        /// </summary>
        public AbilityKey Key { get; }

        /// <summary>Initialises an <see cref="AbilityIntent"/> for the given slot and direction.</summary>
        /// <param name="direction">Desired direction; normalised or <see cref="Vector2.Zero"/>.</param>
        /// <param name="key">Ability slot activated; must not be <see cref="AbilityKey.None"/>.</param>
        public AbilityIntent(Vector2 direction, AbilityKey key) : base(direction)
        {
            Key = key;
        }
    }
}
