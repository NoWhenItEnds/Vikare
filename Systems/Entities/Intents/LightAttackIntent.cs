using Vikare.Entities.Interfaces;

namespace Vikare.Entities.Intents
{
    /// <summary>
    /// An <see cref="IInputIntent"/> expressing a one-shot request to perform a light attack.
    /// Edge-triggered; carries no payload — the target step index is determined by
    /// <see cref="Vikare.Entities.Combat.AttackSequence"/> data at the time the intent is received.
    /// Modelled after <see cref="DodgeIntent"/> as a pure trigger with no direction or charge state.
    /// </summary>
    public sealed class LightAttackIntent : IInputIntent
    {
    }
}
