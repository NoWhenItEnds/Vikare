using Godot;
using Vikare.Entities.Combat;
using Vikare.Entities.Interfaces;
using Vikare.Entities.States;

namespace Vikare.Entities
{
    /// <summary>
    /// A controllable, animated entity; extends <see cref="Entity"/> with <see cref="IAnimated"/>,
    /// <see cref="IStateMachineAccess"/>, and <see cref="IAttacker"/>.
    /// <see cref="IAttacker"/> lives here rather than on <see cref="Entity"/> because melee attacks require
    /// animation playback; non-actor entities (projectiles, props, interactive objects) should not expose
    /// an attack moveset. Restricting the interface to <c>Actor</c> prevents accidental misuse.
    /// </summary>
    public partial class Actor : Entity, IAnimated, IStateMachineAccess, IAttacker
    {
        /// <summary>
        /// Animation player that drives this actor's visual state; delegates from <see cref="PlayAnimation"/>.
        /// </summary>
        /// <remarks>
        /// Populated by Godot from the scene before <c>_Ready</c>; must be wired in the editor.
        /// </remarks>
        [ExportGroup("Nodes")]
        [Export] public AnimationPlayer AnimationPlayer { get; private set; } = null!;

        /// <summary>
        /// State machine governing this actor's behaviour; exposed via <see cref="IStateMachineAccess.Machine"/> for state transitions.
        /// </summary>
        /// <remarks>
        /// Populated by Godot from the scene before <c>_Ready</c>; must be wired in the editor.
        /// </remarks>
        [Export] public StateMachine Machine { get; private set; } = null!;

        /// <summary>
        /// The combo graph this actor uses for melee attacks. Assign a <c>.tres</c> asset in the editor.
        /// Null means no attack moveset is equipped; <see cref="Vikare.Entities.States.AttackingState"/> will
        /// immediately return to idle if this is null when an attack intent arrives.
        /// </summary>
        [ExportGroup("Combat")]
        [Export] public AttackSequence? AttackSequence { get; set; }

        /// <summary>
        /// Power bound to slot 1; null means the slot is empty and <c>cast_power_1</c> input is ignored.
        /// </summary>
        [ExportGroup("Powers")]
        [Export] public PowerDefinition? Power1 { get; set; } = null;

        /// <summary>
        /// Power bound to slot 2; null means the slot is empty and <c>cast_power_2</c> input is ignored.
        /// </summary>
        [Export] public PowerDefinition? Power2 { get; set; } = null;

        /// <summary>
        /// Power bound to slot 3; null means the slot is empty and <c>cast_power_3</c> input is ignored.
        /// </summary>
        [Export] public PowerDefinition? Power3 { get; set; } = null;

        /// <summary>
        /// Power bound to slot 4; null means the slot is empty and <c>cast_power_4</c> input is ignored.
        /// </summary>
        [Export] public PowerDefinition? Power4 { get; set; } = null;

        /// <inheritdoc/>
        /// <remarks>
        /// Base implementation is a no-op virtual hook. Override in a concrete actor subclass or connect a hitbox
        /// system that subscribes to hit-frame events to apply damage, spawn hit effects, etc.
        /// The <paramref name="step"/> parameter is provided for damage data once those fields are added to <see cref="AttackStep"/>.
        /// </remarks>
        public virtual void OnHitFrame(AttackStep step)
        {
        }

        /// <inheritdoc/>
        /// <remarks>
        /// Idempotent: if the named clip is already playing the call is a no-op, preventing looping animations from restarting each tick.
        /// </remarks>
        public void PlayAnimation(string animationName)
        {
            bool alreadyPlaying = AnimationPlayer.CurrentAnimation == animationName
                && AnimationPlayer.IsPlaying();

            if (!alreadyPlaying)
            {
                AnimationPlayer.Play(animationName);
            }
        }
    }
}
