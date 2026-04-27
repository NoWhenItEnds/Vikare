using Godot;
using Godot.Collections;

namespace Vikare.Entities.Abilities
{
    /// <summary>
    /// A single timed node in an ability graph. Unifies what were previously two separate resource
    /// types (<c>AttackStep</c> and <c>PowerDefinition</c>) — both share the same shape (animation,
    /// duration, effect-frame, cancel window, transition map), so one resource type handles both.
    /// Create one asset per distinct animation beat; wire the graph via <see cref="Transitions"/>.
    /// </summary>
    /// <remarks>
    /// Default values represent a short fast-strike: 0.5 s commitment, hit at 0.2 s, chain window
    /// 0.3–0.4 s. Override per asset to match actual animation clip timings.
    /// </remarks>
    [GlobalClass]
    public partial class AbilityStep : Resource
    {
        /// <summary>Animation clip to play when this step becomes active; must exist in the entity's animation library.</summary>
        [Export] public string AnimationName { get; set; } = string.Empty;

        /// <summary>
        /// Total step duration in seconds. Must be positive and greater than <see cref="EffectFrameSeconds"/>
        /// and <see cref="CancelWindowEndSeconds"/>.
        /// </summary>
        [Export] public double DurationSeconds { get; set; } = 0.5;

        /// <summary>
        /// Time offset at which <see cref="Effect"/> fires. Fires at most once per step activation.
        /// Must be less than <see cref="DurationSeconds"/>; ignored when <see cref="Effect"/> is null.
        /// </summary>
        [Export] public double EffectFrameSeconds { get; set; } = 0.2;

        /// <summary>
        /// Earliest time at which a chain <see cref="AbilityIntent"/> is accepted.
        /// Input before this is discarded to prevent buffering during the commitment phase.
        /// Must be less than <see cref="CancelWindowEndSeconds"/>.
        /// </summary>
        [Export] public double CancelWindowStartSeconds { get; set; } = 0.3;

        /// <summary>
        /// Latest time at which a chain <see cref="AbilityIntent"/> is accepted.
        /// Input after this is discarded; the step runs to <see cref="DurationSeconds"/> then returns to idle.
        /// Must be less than <see cref="DurationSeconds"/>.
        /// </summary>
        [Export] public double CancelWindowEndSeconds { get; set; } = 0.4;

        /// <summary>
        /// Effect to execute at <see cref="EffectFrameSeconds"/>. Assign a concrete
        /// <see cref="AbilityEffect"/> subclass asset. Null is valid — the step plays its animation
        /// and transitions without producing any game-world change.
        /// </summary>
        [Export] public AbilityEffect? Effect { get; set; }

        /// <summary>
        /// Maps each ability type (represented by an exemplar <see cref="AbilityEffect"/> instance)
        /// that can chain from this step to the index of the successor step in the owning
        /// <see cref="AbilitySequence.Steps"/> array.
        /// An absent type means that input is silently ignored. Adding a new chain type requires only a
        /// new entry here — no code changes required (OCP).
        /// </summary>
        /// <remarks>
        /// Uses the same exemplar pattern as <see cref="AbilitySequence.EntrySteps"/>: the dictionary
        /// key is any <c>.tres</c> instance of the correct subclass; runtime lookup compares
        /// <c>GetType()</c> via <see cref="TryGetTransition"/>, not reference equality. See
        /// <see cref="AbilitySequence"/> for a full explanation of the serialisation strategy.
        /// </remarks>
        [Export] public Dictionary<AbilityEffect, int> Transitions { get; set; } = new();

        /// <summary>
        /// Finds the chain successor step index for the ability type represented by
        /// <paramref name="ability"/>. Compares by <c>GetType()</c> so any instance of the
        /// matching subclass is a valid key, regardless of which <c>.tres</c> file it was loaded from.
        /// </summary>
        /// <param name="ability">
        /// The ability whose subclass type is used as the lookup key. Typically sourced from
        /// <see cref="Vikare.Entities.AbilityIntent.Ability"/>.
        /// </param>
        /// <param name="stepIndex">
        /// Receives the successor step index when the method returns <c>true</c>; set to <c>-1</c>
        /// (the sentinel) when the method returns <c>false</c>.
        /// </param>
        /// <returns>
        /// <c>true</c> when a matching chain was found; <c>false</c> when the ability type has no
        /// registered transition from this step.
        /// </returns>
        public bool TryGetTransition(AbilityEffect ability, out int stepIndex)
        {
            System.Type abilityType = ability.GetType();
            bool found = false;
            int result = -1;

            foreach (System.Collections.Generic.KeyValuePair<AbilityEffect, int> pair in Transitions)
            {
                bool alreadyFound = found;
                bool typeMatches = pair.Key.GetType() == abilityType;

                if (!alreadyFound && typeMatches)
                {
                    found = true;
                    result = pair.Value;
                }
            }

            stepIndex = result;
            return found;
        }
    }
}
