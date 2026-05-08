using System;
using Godot;

namespace Vikare.Entities.Sensors
{
    /// <summary>
    /// Abstract base class for all physical perception sensors attached to an <see cref="Actor"/>.
    /// <para>
    /// Concrete sensors (e.g. <see cref="SightSensor"/>, <see cref="HearingSensor"/>) inherit this
    /// class and configure their collision shape in <see cref="ConfigureShape"/>. Per-frame work
    /// (such as rotating a cone) is handled by overriding <c>_PhysicsProcess</c> directly in the
    /// subclass — no coordination from <see cref="Actor"/> is required.
    /// </para>
    /// <para>
    /// This class subscribes to <see cref="Area2D.BodyEntered"/> and <see cref="Area2D.BodyExited"/>,
    /// filters for <see cref="Entity"/>-derived bodies, then emits <see cref="EntityDetected"/>
    /// and <see cref="EntityLost"/> so that listeners (e.g. <see cref="Actor"/>) can update their
    /// memory without coupling to any specific sensor implementation.
    /// </para>
    /// <para>
    /// Adding a new sensor type (smell, tremorsense, …) requires only a new subclass — no modification
    /// of this class, <see cref="Actor"/>, or <see cref="Memory.EntityMemory"/>.
    /// </para>
    /// </summary>
    public abstract partial class EntitySensor : Area2D
    {
        /// <summary>
        /// Emitted when an <see cref="Entity"/>-derived body enters the sensor's detection volume.
        /// The actor subscribes to this to call <see cref="Memory.EntityMemory.Remember"/>.
        /// </summary>
        [Signal] public delegate void EntityDetectedEventHandler(Entity entity);

        /// <summary>
        /// Emitted when an <see cref="Entity"/>-derived body exits the sensor's detection volume.
        /// The actor subscribes to this to call <see cref="Memory.EntityMemory.Forget"/>.
        /// </summary>
        [Signal] public delegate void EntityLostEventHandler(Entity entity);

        /// <summary>
        /// Identifies which perceptual channel this sensor represents.
        /// Stored on each <see cref="Memory.SensorMemory"/> for downstream use.
        /// </summary>
        public abstract SensorChannel Channel { get; }


        /// <summary> Configures the collision shape and wires body-entered/exited callbacks. </summary>
        public override void _Ready()
        {
            CollisionLayer = 0;

            // CollisionMask = 1 means this sensor detects bodies on physics layer 1.
            // Ensure all Entity-derived CharacterBody2D nodes are assigned to layer 1 in the
            // project's physics layer settings; adjust this mask if your layer layout differs.
            CollisionMask = 1;

            ConfigureShape();

            BodyEntered += OnBodyEntered;
            BodyExited += OnBodyExited;
        }


        /// <summary> Disconnects body callbacks to prevent signals firing after the node is freed. </summary>
        public override void _ExitTree()
        {
            BodyEntered -= OnBodyEntered;
            BodyExited -= OnBodyExited;
        }


        /// <summary>
        /// Called once in <c>_Ready</c> so the subclass can attach and configure its
        /// <see cref="CollisionShape2D"/> or <see cref="CollisionPolygon2D"/> child node.
        /// </summary>
        protected abstract void ConfigureShape();


        /// <summary>
        /// Guards whether <see cref="EntityDetected"/> is emitted when <paramref name="entity"/>
        /// enters the detection volume via <see cref="Area2D.BodyEntered"/>. The default always
        /// returns true; override in subclasses that need to apply additional entry conditions
        /// (e.g. line-of-sight checks) without altering the base emission logic.
        /// Returning false suppresses the <see cref="EntityDetected"/> signal for this entry event;
        /// the per-frame processing of the subclass is responsible for emitting it once the
        /// condition clears.
        /// </summary>
        /// <param name="entity"> The entity that geometrically entered the detection volume. </param>
        /// <returns> True when the signal should be emitted; false to suppress it. </returns>
        protected virtual Boolean ShouldEmitDetection(Entity entity)
        {
            return true;
        }


        /// <summary>
        /// Handles <see cref="Area2D.BodyEntered"/>; filters for <see cref="Entity"/>
        /// and emits <see cref="EntityDetected"/> when the entering body qualifies and
        /// <see cref="ShouldEmitDetection"/> permits it.
        /// </summary>
        /// <param name="body"> The physics body that entered the sensor volume. </param>
        private void OnBodyEntered(Node2D body)
        {
            if (body is Entity entity && ShouldEmitDetection(entity))
            {
                EmitSignal(SignalName.EntityDetected, entity);
            }
        }


        /// <summary>
        /// Handles <see cref="Area2D.BodyExited"/>; filters for <see cref="Entity"/>
        /// and emits <see cref="EntityLost"/> when the exiting body qualifies.
        /// </summary>
        /// <param name="body"> The physics body that exited the sensor volume. </param>
        private void OnBodyExited(Node2D body)
        {
            if (body is Entity entity)
            {
                EmitSignal(SignalName.EntityLost, entity);
            }
        }
    }
}
