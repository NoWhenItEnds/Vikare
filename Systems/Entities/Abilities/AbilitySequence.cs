using Godot;
using Godot.Collections;

namespace Vikare.Entities.Abilities
{
    /// <summary>
    /// The full ability graph for a character's moveset. Contains all <see cref="AbilityStep"/> nodes
    /// and a set of named entry points mapping each initiating <see cref="AbilityKey"/> to the index of
    /// the first step in the corresponding sub-graph.
    /// Create one <c>.tres</c> asset per character moveset or weapon; no code changes are required to
    /// add new movesets or entry-point slots (OCP).
    /// </summary>
    [GlobalClass]
    public partial class AbilitySequence : Resource
    {
        /// <summary>
        /// All steps in this ability graph. Steps reference each other by index via
        /// <see cref="AbilityStep.Transitions"/>; the order of this array is the canonical index space.
        /// </summary>
        [Export] public Array<AbilityStep> Steps { get; set; } = new();

        /// <summary>
        /// Maps each <see cref="AbilityKey"/> (as its raw integer value) that can initiate this sequence
        /// to the index of its root step within <see cref="Steps"/>. An absent key means that input
        /// will not start this sequence.
        /// </summary>
        /// <remarks>
        /// Godot cannot serialise a typed enum key in an <c>[Export]</c> dictionary, so the key is the
        /// raw integer value of the <see cref="AbilityKey"/> enum. In the editor the integers map
        /// directly to the enum's numeric values. An unrecognised key silently produces no effect —
        /// double-check integer values against <see cref="AbilityKey"/> when an ability fails to trigger.
        /// </remarks>
        [Export] public Dictionary<int, int> EntrySteps { get; set; } = new();
    }
}
