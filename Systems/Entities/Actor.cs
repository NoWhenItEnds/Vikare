using Godot;
using Vikare.Entities.Interfaces;
using Vikare.Entities.States;

namespace Vikare.Entities
{
    /// <summary>
    /// A controllable, animated entity; extends <see cref="Entity"/> with <see cref="IAnimated"/> and <see cref="IStateMachineAccess"/>.
    /// </summary>
    public partial class Actor : Entity, IAnimated, IStateMachineAccess
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
