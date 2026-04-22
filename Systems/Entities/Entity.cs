using Godot;
using Vikare.Entities.Interfaces;

namespace Vikare.Entities
{
    /// <summary>
    /// Base class for all interactive entities; inherits from <see cref="CharacterBody2D"/> and implements <see cref="IMovable"/> and <see cref="IGrounded"/>.
    /// </summary>
    public partial class Entity : CharacterBody2D, IMovable, IGrounded
    {
        /// <summary>
        /// The entity's primary collision shape; must be assigned in the editor.
        /// </summary>
        /// <remarks>
        /// Populated by Godot from the scene before <c>_Ready</c>; must be wired in the editor.
        /// </remarks>
        [ExportGroup("Nodes")]
        [Export] public CollisionShape2D Collision { get; private set; } = null!;

        /// <summary>
        /// Maximum travel speed for normal walking movement, in pixels per second; must be positive.
        /// </summary>
        [ExportGroup("Movement")]
        [Export] public float MaxSpeed { get; set; } = 200f;

        /// <summary>
        /// Maximum travel speed whilst sprinting, in pixels per second; should exceed <see cref="MaxSpeed"/>.
        /// </summary>
        [Export] public float MaxSprintSpeed { get; set; } = 400f;

        /// <summary>
        /// Travel speed in pixels per second during a dodge burst; should exceed <see cref="MaxSprintSpeed"/> to feel responsive.
        /// </summary>
        [Export] public float MaxDodgeSpeed { get; set; } = 600f;

        /// <summary>
        /// Duration of a single dodge burst in seconds; the dodge state transitions to idle once this threshold is exceeded.
        /// </summary>
        [Export] public float MaxDodgeDurationSeconds { get; set; } = 0.25f;

        /// <inheritdoc/>
        /// <remarks>
        /// Backed by <c>CharacterBody2D.Velocity</c> so Godot's physics helpers remain consistent with the value states write here.
        /// </remarks>
        public Vector2 MovementVelocity
        {
            get => Velocity;
            set => Velocity = value;
        }

        /// <inheritdoc/>
        public bool IsOnGround => IsOnFloor();

        /// <inheritdoc/>
        public void ZeroVelocity()
        {
            MovementVelocity = Vector2.Zero;
        }

        /// <summary>
        /// Sealed physics tick entry point; calls <see cref="OnPhysicsTick"/> then <c>MoveAndSlide</c>.
        /// </summary>
        /// <remarks>
        /// Godot invokes <c>_PhysicsProcess</c> parent-before-child, so this method calls <c>MoveAndSlide</c>
        /// before the child <c>StateMachine._PhysicsProcess</c> runs. Velocity written by the active state at tick N
        /// is therefore consumed at tick N+1. Subclasses must override <see cref="OnPhysicsTick"/> rather than <c>_PhysicsProcess</c>.
        /// </remarks>
        /// <param name="delta">Elapsed time since the last physics tick, in seconds.</param>
        public sealed override void _PhysicsProcess(double delta)
        {
            OnPhysicsTick(delta);
            MoveAndSlide();
        }

        /// <summary>
        /// Per-physics-tick hook for subclasses; called before <c>MoveAndSlide</c>. Override to apply gravity or knockback.
        /// </summary>
        /// <param name="delta">Elapsed time since the last physics tick, in seconds.</param>
        protected virtual void OnPhysicsTick(double delta)
        {
        }
    }
}
