using Vikare.Entities.Combat;
using Vikare.Entities.Interfaces;

namespace Vikare.Entities.Intents
{
    /// <summary>
    /// An <see cref="IInputIntent"/> expressing a request to cast a specific power.
    /// Carries a <see cref="PowerDefinition"/> reference so the casting state knows which animation,
    /// duration, and effect to apply — no magic strings, no switch-dispatch.
    /// </summary>
    public sealed class CastPowerIntent : IInputIntent
    {
        /// <summary>
        /// The power to cast. May be null when the controller is in an indeterminate state;
        /// the transition table guards against null via its predicate, and <see cref="Vikare.Entities.States.CastingPowerState"/>
        /// guards again in <c>HandleIntent</c> for safety.
        /// </summary>
        public PowerDefinition? Power { get; }

        /// <summary>
        /// Initialises a new <see cref="CastPowerIntent"/> carrying the supplied power definition.
        /// </summary>
        /// <param name="power">The power to cast; pass null only when no power is selected.</param>
        public CastPowerIntent(PowerDefinition? power)
        {
            Power = power;
        }
    }
}
