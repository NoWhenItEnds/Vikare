using Godot;
using Godot.Collections;

namespace Vikare.Entities.Combat
{
    /// <summary>
    /// The full combo graph for a weapon or moveset. Each element in <see cref="Steps"/> is an <see cref="AttackStep"/>;
    /// steps reference each other by index to form an acyclic directed graph of attack chains.
    /// Create a new <c>.tres</c> asset for each distinct weapon or character moveset — no code changes required.
    /// </summary>
    [GlobalClass]
    public partial class AttackSequence : Resource
    {
        /// <summary>
        /// All steps available in this combo graph, indexed by the <c>NextOnLight</c>/<c>NextOnHeavy</c>
        /// fields on each <see cref="AttackStep"/>. Order matters — index 0 is step zero, etc.
        /// </summary>
        [Export] public Array<AttackStep> Steps { get; set; } = new();

        /// <summary>
        /// Index into <see cref="Steps"/> that the combo begins at when a
        /// <see cref="Vikare.Entities.Controllers.LightAttackIntent"/> initiates the sequence.
        /// Set to <c>-1</c> if this moveset has no light-attack entry point.
        /// </summary>
        [Export] public int RootOnLight { get; set; } = 0;

        /// <summary>
        /// Index into <see cref="Steps"/> that the combo begins at when a
        /// <see cref="Vikare.Entities.Controllers.HeavyAttackIntent"/> initiates the sequence.
        /// Set to <c>-1</c> if this moveset has no heavy-attack entry point.
        /// </summary>
        [Export] public int RootOnHeavy { get; set; } = -1;
    }
}
