using System;
using System.Collections.Generic;
using Godot;
using Godot.Collections;

namespace Vikare.Entities.Sensors
{
    /// <summary>
    /// Directional vision cone sensor. Detects <see cref="Entity"/>-derived bodies within a
    /// configurable arc centred on the owning <see cref="Actor"/>'s facing direction.
    /// Attach as a child of an <see cref="Actor"/> node.
    /// <para>
    /// The cone polygon is built once in <c>_Ready</c> facing the +Y axis (down at angle 0).
    /// Each physics frame <c>_PhysicsProcess</c> rotates the node to align that +Y forward axis
    /// with <c>Actor.Direction</c>, so no polygon rebuild is needed on each frame.
    /// </para>
    /// <para>
    /// When <see cref="RequireLineOfSight"/> is <c>true</c>, each physics frame the sensor
    /// ray-casts from its own <see cref="Node2D.GlobalPosition"/> to each overlapping entity's
    /// <see cref="Node2D.GlobalPosition"/> using the <see cref="OcclusionMask"/> physics layers
    /// to test for blocking geometry. Entities that are geometrically inside the cone but behind
    /// an occluder are not reported as detected. State transitions (visible ↔ occluded) emit
    /// <see cref="EntitySensor.EntityDetected"/> and <see cref="EntitySensor.EntityLost"/>
    /// respectively.
    /// </para>
    /// <para>
    /// When <see cref="RequireLineOfSight"/> is <c>false</c> (the default), behaviour is
    /// identical to the pre-occlusion implementation: geometric overlap alone determines
    /// detection, with zero per-frame ray-cast cost.
    /// </para>
    /// <para>
    /// Expected scene tree position:
    /// <code>
    /// Actor (Actor.cs)
    ///   └── SightSensor (SightSensor.cs)   ← this node
    /// </code>
    /// </para>
    /// </summary>
    public partial class SightSensor : EntitySensor
    {
        /// <summary>
        /// Half-angle of the vision cone in degrees. The full cone spans
        /// <c>HalfAngleDegrees * 2</c> degrees symmetrically around the facing direction.
        /// Valid range: 1–179. Defaults to 60° half-angle (120° total field of view).
        /// </summary>
        [Export] public Single HalfAngleDegrees { get; set; } = 60f;

        /// <summary>
        /// Maximum sight range in world-space pixels; arc vertices are placed at this distance
        /// from the actor's origin.
        /// </summary>
        [Export] public Single Range { get; set; } = 256f;

        /// <summary>
        /// Number of arc segments used to approximate the cone boundary. Higher values give a
        /// smoother arc at the cost of a larger polygon. Must be at least 1.
        /// </summary>
        [Export] public Int32 ArcSegments { get; set; } = 12;

        /// <summary>
        /// When <c>true</c>, entities that are geometrically inside the cone but occluded by
        /// physics bodies on <see cref="OcclusionMask"/> layers are not reported as detected.
        /// Defaults to <c>false</c> to preserve the original overlap-only behaviour for any
        /// actor that does not opt in.
        /// </summary>
        [Export] public Boolean RequireLineOfSight { get; set; } = false;

        /// <summary>
        /// Physics layers that block sight when <see cref="RequireLineOfSight"/> is <c>true</c>.
        /// Configure this in the inspector to include your wall and terrain layers.
        /// Ignored when <see cref="RequireLineOfSight"/> is <c>false</c>.
        /// </summary>
        [Export(PropertyHint.Layers2DPhysics)] public UInt32 OcclusionMask { get; set; } = 0;


        /// <summary> Cached reference to the actor that owns this sensor; resolved in <c>_Ready</c>. </summary>
        private Actor? _owner;

        /// <summary>
        /// Tracks entities that are currently both geometrically inside the cone and have a clear
        /// line of sight to this sensor. Used only when <see cref="RequireLineOfSight"/> is
        /// <c>true</c>; remains empty otherwise.
        /// </summary>
        private readonly HashSet<Entity> _visibleEntities = new HashSet<Entity>();

        /// <summary>
        /// Reusable working set for the current frame's ray-cast results inside
        /// <see cref="UpdateOcclusionState"/>. Pre-allocated to avoid per-frame heap pressure;
        /// cleared at the start of each call before being repopulated.
        /// </summary>
        private readonly HashSet<Entity> _nowVisible = new HashSet<Entity>();

        /// <summary>
        /// Single-element exclude list passed to every ray-cast query to prevent the actor's own
        /// <see cref="CharacterBody2D"/> from blocking its own sight rays. Populated once in
        /// <c>_Ready</c> with <see cref="_owner"/>'s <see cref="GodotObject.GetRid"/>; remains
        /// empty (and harmless) when <see cref="_owner"/> is null.
        /// </summary>
        private readonly Array<Rid> _rayExclude = new Array<Rid>();

        /// <summary>
        /// Cached delegate for the <see cref="Area2D.BodyExited"/> subscription added when
        /// <see cref="RequireLineOfSight"/> is <c>true</c>. Stored so the same delegate
        /// instance can be removed in <see cref="_ExitTree"/>.
        /// </summary>
        private BodyExitedEventHandler? _bodyExitedCleanupHandler;


        /// <inheritdoc/>
        public override SensorChannel Channel => SensorChannel.Sight;


        /// <summary>
        /// Clears runtime state, caches the owning actor reference, delegates to the base to
        /// configure the shape, populates the ray-cast exclude list with the actor's RID, and —
        /// when <see cref="RequireLineOfSight"/> is <c>true</c> — subscribes the visibility-state
        /// cleanup handler to <see cref="Area2D.BodyExited"/>.
        /// The runtime-state clears before <c>base._Ready()</c> mirror the discipline used by
        /// <c>Actor._Ready</c> for defence-in-depth against re-parenting cycles.
        /// </summary>
        public override void _Ready()
        {
            _visibleEntities.Clear();
            _nowVisible.Clear();
            _rayExclude.Clear();

            _owner = GetParentOrNull<Actor>();
            base._Ready();

            if (_owner != null)
            {
                _rayExclude.Add(_owner.GetRid());
            }

            if (RequireLineOfSight)
            {
                _bodyExitedCleanupHandler = OnBodyExitedCleanup;
                BodyExited += _bodyExitedCleanupHandler;
            }
        }


        /// <summary>
        /// Rotates the sensor cone to track <see cref="Actor.Direction"/> and, when
        /// <see cref="RequireLineOfSight"/> is <c>true</c>, sweeps all overlapping entities with a
        /// ray cast and emits <see cref="EntitySensor.EntityDetected"/> or
        /// <see cref="EntitySensor.EntityLost"/> on visibility state transitions.
        /// </summary>
        /// <param name="delta"> Elapsed time in seconds since the last physics frame (not used; rotation is set directly). </param>
        public override void _PhysicsProcess(Double delta)
        {
            if (_owner != null)
            {
                Rotation = MathF.Atan2(-_owner.Direction.X, _owner.Direction.Y);
            }

            if (RequireLineOfSight)
            {
                UpdateOcclusionState();
            }
        }


        /// <summary>
        /// Disconnects the occlusion cleanup handler (when <see cref="RequireLineOfSight"/> is
        /// <c>true</c>), clears all runtime state sets so stale entries do not survive a
        /// <c>Reparent</c> or scene-reload cycle, and delegates to the base.
        /// </summary>
        public override void _ExitTree()
        {
            if (_bodyExitedCleanupHandler != null)
            {
                BodyExited -= _bodyExitedCleanupHandler;
                _bodyExitedCleanupHandler = null;
            }

            _visibleEntities.Clear();
            _nowVisible.Clear();
            _rayExclude.Clear();

            base._ExitTree();
        }


        /// <summary>
        /// Builds the cone polygon facing the +Y axis and attaches it as a <see cref="CollisionPolygon2D"/> child.
        /// The polygon origin sits at the actor's centre; per-frame rotation in <see cref="_PhysicsProcess"/>
        /// tracks the actor's facing direction.
        /// </summary>
        protected override void ConfigureShape()
        {
            Int32 segments = Math.Max(1, ArcSegments);
            Single halfAngleRad = Mathf.DegToRad(HalfAngleDegrees);

            // points[0] is the cone apex; points[1..segments+1] trace the arc.
            Vector2[] points = new Vector2[segments + 2];
            points[0] = Vector2.Zero;

            for (Int32 i = 0; i <= segments; i++)
            {
                Single t = (Single)i / segments;
                Single angle = Mathf.Lerp(-halfAngleRad, halfAngleRad, t);

                // +Y is the forward axis; X component gives lateral spread.
                points[i + 1] = new Vector2(MathF.Sin(angle) * Range, MathF.Cos(angle) * Range);
            }

            CollisionPolygon2D polygon = new CollisionPolygon2D();
            polygon.Polygon = points;

            AddChild(polygon);
        }


        /// <summary>
        /// Returns <c>false</c> when <see cref="RequireLineOfSight"/> is <c>true</c> and a ray
        /// from this sensor to <paramref name="entity"/> is blocked by <see cref="OcclusionMask"/>
        /// geometry. Called by the base class <c>OnBodyEntered</c> handler before emitting
        /// <see cref="EntitySensor.EntityDetected"/>. When <see cref="RequireLineOfSight"/> is
        /// <c>false</c>, delegates to the base (always <c>true</c>).
        /// </summary>
        /// <param name="entity"> The entity that geometrically entered the cone. </param>
        /// <returns>
        /// <c>true</c> when the entity should be reported as detected immediately on entry;
        /// <c>false</c> when the signal should be suppressed because the entity is currently
        /// occluded (the per-frame sweep will emit it once the ray clears).
        /// </returns>
        protected override Boolean ShouldEmitDetection(Entity entity)
        {
            Boolean baseResult = base.ShouldEmitDetection(entity);

            Boolean result;

            if (RequireLineOfSight)
            {
                Boolean hasLineOfSight = HasLineOfSightTo(entity);

                if (hasLineOfSight)
                {
                    _visibleEntities.Add(entity);
                }

                result = baseResult && hasLineOfSight;
            }
            else
            {
                result = baseResult;
            }

            return result;
        }


        /// <summary>
        /// Iterates all overlapping bodies, ray-casts to each <see cref="Entity"/>, and emits
        /// <see cref="EntitySensor.EntityDetected"/> or <see cref="EntitySensor.EntityLost"/>
        /// on transitions between visible and occluded states.
        /// <para>
        /// <see cref="EntitySensor.EntityLost"/> transitions are processed before
        /// <see cref="EntitySensor.EntityDetected"/> transitions within the same frame so that
        /// consumers tracking a count of currently visible entities decrement before
        /// incrementing, preserving a consistent intermediate state.
        /// </para>
        /// <para>
        /// Uses the pre-allocated <see cref="_nowVisible"/> working set to avoid per-frame
        /// heap allocation; the set is cleared at the top of each call.
        /// </para>
        /// </summary>
        private void UpdateOcclusionState()
        {
            Array<Node2D> overlapping = GetOverlappingBodies();

            // Populate the working set before emitting any signals so signal handlers
            // observe a consistent snapshot rather than a partially-updated state.
            _nowVisible.Clear();

            foreach (Node2D body in overlapping)
            {
                if (body is Entity entity && HasLineOfSightTo(entity))
                {
                    _nowVisible.Add(entity);
                }
            }

            // Emit EntityLost for entities that were visible last frame but are now occluded.
            foreach (Entity entity in _visibleEntities)
            {
                if (!_nowVisible.Contains(entity))
                {
                    EmitSignal(SignalName.EntityLost, entity);
                }
            }

            // Emit EntityDetected for entities that were not visible last frame but are now clear.
            foreach (Entity entity in _nowVisible)
            {
                if (!_visibleEntities.Contains(entity))
                {
                    EmitSignal(SignalName.EntityDetected, entity);
                }
            }

            _visibleEntities.Clear();

            foreach (Entity entity in _nowVisible)
            {
                _visibleEntities.Add(entity);
            }
        }


        /// <summary>
        /// Ray-casts from this sensor's <see cref="Node2D.GlobalPosition"/> to
        /// <paramref name="entity"/>'s <see cref="Node2D.GlobalPosition"/> using
        /// <see cref="OcclusionMask"/>. The actor's own body is excluded via
        /// <see cref="_rayExclude"/> to prevent it from blocking its own sight rays.
        /// Returns <c>true</c> when no blocking body is hit.
        /// </summary>
        /// <param name="entity"> The entity to test visibility towards. </param>
        /// <returns> <c>true</c> when the ray reaches the entity unobstructed. </returns>
        private Boolean HasLineOfSightTo(Entity entity)
        {
            PhysicsDirectSpaceState2D spaceState = GetWorld2D().DirectSpaceState;

            PhysicsRayQueryParameters2D query = new PhysicsRayQueryParameters2D
            {
                From = GlobalPosition,
                To = entity.GlobalPosition,
                CollisionMask = OcclusionMask,
                CollideWithAreas = false,
                CollideWithBodies = true,
                Exclude = _rayExclude,
            };

            Dictionary result = spaceState.IntersectRay(query);

            return result.Count == 0;
        }


        /// <summary>
        /// Handles the second <see cref="Area2D.BodyExited"/> subscription added when
        /// <see cref="RequireLineOfSight"/> is <c>true</c>. Removes the exiting body from
        /// <see cref="_visibleEntities"/> so the per-frame sweep does not hold stale references
        /// to bodies that have already left the cone geometry.
        /// The base class subscription to <see cref="Area2D.BodyExited"/> still fires independently
        /// and emits <see cref="EntitySensor.EntityLost"/> — this handler does not re-emit it.
        /// </summary>
        /// <param name="body"> The physics body that exited the cone volume. </param>
        private void OnBodyExitedCleanup(Node2D body)
        {
            if (body is Entity entity)
            {
                _visibleEntities.Remove(entity);
            }
        }
    }
}
