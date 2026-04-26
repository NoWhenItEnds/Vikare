using Godot;
using Vikare.Entities.Combat;
using Vikare.Entities.States;
using Vikare.Managers;
using Vikare.Utilities.Singletons;

namespace Vikare.Entities
{
    /// <summary>
    /// A controllable, animated entity. Extends <see cref="Entity"/> with animation playback,
    /// a state machine reference, an ability sequence, and optional player-input registration.
    /// </summary>
    /// <remarks>
    /// When <see cref="IsPlayerControlled"/> is true, the actor self-registers with
    /// <see cref="InputManager"/> in <c>_Ready</c> and de-registers in <c>_ExitTree</c>.
    /// Any entity in the scene can be made player-controlled by setting this flag in the editor.
    /// </remarks>
    public partial class Actor : Entity
    {
        /// <summary>Animation player driving this actor's visual state; must be wired in the editor.</summary>
        [ExportGroup("Nodes")]
        [Export] public AnimationPlayer AnimationPlayer { get; private set; } = null!;

        /// <summary>State machine governing this actor's behaviour; must be wired in the editor.</summary>
        [Export] public StateMachine Machine { get; private set; } = null!;

        /// <summary>
        /// Ability graph used for melee attacks, power casts, and combo chains. Assign a <c>.tres</c>
        /// asset in the editor. Null means no moveset is equipped; <see cref="AbilityState"/> returns
        /// to idle immediately when this is null.
        /// </summary>
        [ExportGroup("Combat")]
        [Export] public AbilitySequence? AbilitySequence { get; set; }

        /// <summary>
        /// When true, this actor registers with <see cref="InputManager"/> and receives player input.
        /// Only one actor should have this set at a time.
        /// </summary>
        [ExportGroup("Player")]
        [Export] public bool IsPlayerControlled { get; set; } = false;

        /// <summary>
        /// Registers with <see cref="InputManager"/> when <see cref="IsPlayerControlled"/> is true.
        /// Registration happens here rather than in the constructor because
        /// <see cref="SingletonNode{T}.Instance"/> is only valid after the manager node has entered the tree.
        /// </summary>
        public override void _Ready()
        {
            base._Ready();

            if (IsPlayerControlled)
            {
                InputManager.Instance.RegisterPlayer(this);
            }
        }

        /// <summary>
        /// De-registers from <see cref="InputManager"/> when <see cref="IsPlayerControlled"/> is true.
        /// Uses an <c>as</c> cast because the singleton's backing field may be null during scene
        /// teardown even though the property is declared non-nullable.
        /// </summary>
        public override void _ExitTree()
        {
            InputManager? manager = InputManager.Instance as InputManager;
            bool shouldDeregister = IsPlayerControlled && manager is not null;
            if (shouldDeregister)
            {
                manager!.DeregisterPlayer();
            }

            base._ExitTree();
        }

        /// <summary>
        /// Plays the named animation clip. No-op if that clip is already playing, preventing looping
        /// animations from restarting each tick.
        /// </summary>
        /// <param name="animationName">Clip name as defined in the animation library.</param>
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
