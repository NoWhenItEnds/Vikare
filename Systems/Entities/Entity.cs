using Godot;

namespace Vikare.Entities
{
    /// <summary>
    /// Base class for all interactive entities. Extends <see cref="CharacterBody2D"/> with movement
    /// properties, ground detection, and a sealed physics loop that guarantees <c>MoveAndSlide</c>
    /// always runs.
    /// </summary>
    public partial class Entity : CharacterBody2D
    {
        /// <summary>The entity's primary collision shape; must be wired in the editor.</summary>
        [ExportGroup("Nodes")]
        [Export] public CollisionShape2D Collision { get; private set; } = null!;

        /// <summary>Maximum walk speed in pixels per second.</summary>
        [ExportGroup("Movement")]
        [Export] public float MaxSpeed { get; set; } = 200f;

        /// <summary>Maximum sprint speed in pixels per second; should exceed <see cref="MaxSpeed"/>.</summary>
        [Export] public float MaxSprintSpeed { get; set; } = 400f;

        /// <summary>Speed during a dodge burst in pixels per second; should exceed <see cref="MaxSprintSpeed"/>.</summary>
        [Export] public float MaxDodgeSpeed { get; set; } = 600f;

        /// <summary>Duration of a single dodge burst in seconds.</summary>
        [Export] public float MaxDodgeDurationSeconds { get; set; } = 0.25f;

        /// <summary>
        /// Velocity applied each physics tick. Backed by <c>CharacterBody2D.Velocity</c> so Godot's
        /// physics helpers remain consistent with the value states write here.
        /// </summary>
        public Vector2 MovementVelocity
        {
            get => Velocity;
            set => Velocity = value;
        }

        /// <summary>True when the entity is resting on a floor surface; updated after each <c>MoveAndSlide</c>.</summary>
        public bool IsOnGround => IsOnFloor();

        /// <summary>Sets <see cref="MovementVelocity"/> to zero.</summary>
        public void ZeroVelocity()
        {
            MovementVelocity = Vector2.Zero;
        }

        /// <summary>
        /// Calls <see cref="OnPhysicsTick"/> then <c>MoveAndSlide</c>. Sealed so <c>MoveAndSlide</c>
        /// always runs; subclasses override <see cref="OnPhysicsTick"/> instead.
        /// </summary>
        /// <param name="delta">Elapsed time since the last physics tick, in seconds.</param>
        public sealed override void _PhysicsProcess(double delta)
        {
            OnPhysicsTick(delta);
            MoveAndSlide();
        }

        /// <summary>
        /// Per-physics-tick hook called before <c>MoveAndSlide</c>. Override to apply gravity or knockback.
        /// Default implementation is empty.
        /// </summary>
        /// <param name="delta">Elapsed time since the last physics tick, in seconds.</param>
        protected virtual void OnPhysicsTick(double delta)
        {
        }
    }
}
