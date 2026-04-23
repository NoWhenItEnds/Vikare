using Vikare.Managers;
using Vikare.Utilities.Singletons;

namespace Vikare.Entities
{
    /// <summary>
    /// The player-controlled actor; registers itself with <see cref="InputManager"/> so that
    /// translated input intents are forwarded to this actor's state machine each frame.
    /// </summary>
    public partial class Player : Actor
    {
        /// <inheritdoc/>
        /// <remarks>
        /// Registers this actor as the input recipient immediately after the base node hierarchy
        /// is ready. Registration must happen in <c>_Ready</c> rather than the constructor because
        /// <see cref="InputManager.Instance"/> is only valid once the manager node has entered the
        /// scene tree; constructors run before scene-tree insertion.
        /// </remarks>
        public override void _Ready()
        {
            base._Ready();
            InputManager.Instance.SetPlayer(this);
        }

        /// <inheritdoc/>
        /// <remarks>
        /// Clears this actor from <see cref="InputManager"/> before the node leaves the tree.
        /// A null guard is required because Godot's scene teardown order is not guaranteed —
        /// <see cref="InputManager"/> (a <see cref="SingletonNode{T}"/>) may have already called
        /// <c>ClearIfMatch</c> and nulled its internal <c>_instance</c> field before this node exits.
        /// <see cref="SingletonNode{T}.Instance"/> delegates to <c>SingletonHelper._instance!</c>,
        /// which suppresses the nullable warning but can return a null reference at runtime during
        /// teardown. The <c>is { }</c> pattern safely catches that case without throwing.
        /// </remarks>
        public override void _ExitTree()
        {
            if (InputManager.Instance is { } manager)
            {
                manager.ClearPlayer();
            }

            base._ExitTree();
        }
    }
}
