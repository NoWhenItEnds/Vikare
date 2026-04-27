using Godot;
using Godot.Collections;
using System.Linq;

namespace Vikare.Entities.Abilities
{
    /// <summary>
    /// The full ability graph for a character's moveset. Contains all <see cref="AbilityStep"/> nodes
    /// and a set of named entry points mapping each initiating ability type to the index of the first
    /// step in the corresponding sub-graph.
    /// Create one <c>.tres</c> asset per character moveset or weapon; no code changes are required to
    /// add new movesets or entry-point slots (OCP).
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Serialisation strategy — exemplar resources:</b> Godot 4 cannot serialise
    /// <c>System.Type</c> directly via <c>[Export]</c>. This class uses the
    /// <em>per-class exemplar</em> pattern as the bridge between the editor and
    /// the runtime type system:
    /// </para>
    /// <list type="bullet">
    ///   <item>
    ///     In the editor, the designer creates a <c>.tres</c> asset of the appropriate
    ///     <see cref="AbilityEffect"/> subclass (e.g. a <c>MeleeHitAbilityEffect.tres</c>) and
    ///     places it as a key in <see cref="EntrySteps"/>.
    ///   </item>
    ///   <item>
    ///     At runtime, <see cref="TryGetEntryStep"/> compares types via <c>GetType()</c>, not
    ///     reference equality. Two different <c>.tres</c> files that are both
    ///     <c>MeleeHitAbilityEffect</c> instances are treated as identical keys — the subclass
    ///     itself is the canonical identity.
    ///   </item>
    ///   <item>
    ///     The exemplar's exported property values (e.g. <c>BaseDamage</c>) are irrelevant to
    ///     the lookup; any instance of the correct subclass acts as a valid key.
    ///   </item>
    /// </list>
    /// <para>
    /// This approach was chosen over <c>Array&lt;Script&gt;</c> keys because it keeps
    /// the inspector experience type-safe and self-documenting: the designer sees the
    /// concrete effect class directly, rather than a raw script reference. It also avoids
    /// the overhead of Godot-to-C# script-to-type resolution at lookup time.
    /// </para>
    /// </remarks>
    [GlobalClass]
    public partial class AbilitySequence : Resource
    {
        /// <summary>
        /// All steps in this ability graph. Steps reference each other by index via
        /// <see cref="AbilityStep.Transitions"/>; the order of this array is the canonical index space.
        /// </summary>
        [Export] public Array<AbilityStep> Steps { get; set; } = new();

        /// <summary>
        /// Maps each ability type (represented by an exemplar <see cref="AbilityEffect"/> instance)
        /// to the index of its root step within <see cref="Steps"/>. The exemplar is used purely as a
        /// type token — its property values do not affect the lookup. An absent type means that input
        /// will not start this sequence.
        /// </summary>
        /// <remarks>
        /// Use <see cref="TryGetEntryStep"/> for all runtime lookups. Do not query this dictionary
        /// directly, as that would apply reference equality rather than the required type equality.
        /// </remarks>
        [Export] public Dictionary<AbilityEffect, int> EntrySteps { get; set; } = new();

        /// <summary>
        /// Finds the root step index for the ability type represented by <paramref name="ability"/>.
        /// Compares by <c>GetType()</c> so any instance of the matching subclass is a valid key,
        /// regardless of which <c>.tres</c> file it was loaded from.
        /// </summary>
        /// <param name="ability">
        /// The ability whose subclass type is used as the lookup key. Typically sourced from
        /// <see cref="Vikare.Entities.AbilityIntent.Ability"/>.
        /// </param>
        /// <param name="stepIndex">
        /// Receives the root step index when the method returns <c>true</c>; set to <c>-1</c>
        /// (the sentinel) when the method returns <c>false</c>.
        /// </param>
        /// <returns>
        /// <c>true</c> when a matching entry was found; <c>false</c> when the ability type has no
        /// registered entry point in this sequence.
        /// </returns>
        public bool TryGetEntryStep(AbilityEffect ability, out int stepIndex)
        {
            System.Type abilityType = ability.GetType();
            bool found = false;
            int result = -1;

            foreach (System.Collections.Generic.KeyValuePair<AbilityEffect, int> pair in EntrySteps)
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
