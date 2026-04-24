using Godot;

namespace Vikare.Entities.Combat
{
    /// <summary>
    /// A single node in an <see cref="AttackSequence"/> combo graph.
    /// Create one <see cref="AttackStep"/> resource per distinct attack animation beat;
    /// chain them by setting <see cref="NextOnLight"/> and <see cref="NextOnHeavy"/> to indices
    /// within the owning <see cref="AttackSequence.Steps"/> array.
    /// Adding a new combo link never requires editing code — only resource data changes.
    /// </summary>
    [GlobalClass]
    public partial class AttackStep : Resource
    {
        /// <summary>
        /// Name of the animation clip to play for this step; must match a clip in the entity's animation library.
        /// </summary>
        [Export] public string AnimationName { get; set; } = string.Empty;

        /// <summary>
        /// Total duration of this step in seconds. If no chain input is accepted before this elapses,
        /// <see cref="Vikare.Entities.States.AttackingState"/> transitions to idle.
        /// The effective duration is rounded up to the next physics tick.
        /// </summary>
        [Export] public double DurationSeconds { get; set; } = 0.5;

        /// <summary>
        /// Time offset in seconds from the start of this step at which <see cref="Vikare.Entities.Interfaces.IAttacker.OnHitFrame"/> fires.
        /// Must be less than <see cref="DurationSeconds"/>; fired at most once per step activation.
        /// </summary>
        [Export] public double HitFrameSeconds { get; set; } = 0.2;

        /// <summary>
        /// Earliest time in seconds at which a chain intent (<see cref="Vikare.Entities.Intents.LightAttackIntent"/>
        /// or <see cref="Vikare.Entities.Intents.HeavyAttackIntent"/>) is accepted.
        /// Input arriving before this offset is ignored to prevent accidental buffering.
        /// </summary>
        [Export] public double CancelWindowStartSeconds { get; set; } = 0.3;

        /// <summary>
        /// Latest time in seconds at which a chain intent is accepted.
        /// Input arriving after this offset is ignored; the step runs to <see cref="DurationSeconds"/> and returns to idle.
        /// </summary>
        [Export] public double CancelWindowEndSeconds { get; set; } = 0.45;

        /// <summary>
        /// Index into <see cref="AttackSequence.Steps"/> identifying the step to transition to when a
        /// <see cref="Vikare.Entities.Intents.LightAttackIntent"/> arrives in the cancel window.
        /// Set to <c>-1</c> to indicate no light-attack chain is available from this step.
        /// </summary>
        [Export] public int NextOnLight { get; set; } = -1;

        /// <summary>
        /// Index into <see cref="AttackSequence.Steps"/> identifying the step to transition to when a
        /// <see cref="Vikare.Entities.Intents.HeavyAttackIntent"/> arrives in the cancel window.
        /// Set to <c>-1</c> to indicate no heavy-attack chain is available from this step.
        /// </summary>
        [Export] public int NextOnHeavy { get; set; } = -1;
    }
}
