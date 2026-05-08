using System;
using Godot;

namespace Vikare.Entities.Sensors
{
    /// <summary>
    /// Omnidirectional hearing sensor. Detects any <see cref="Entity"/>-derived body within a
    /// circular radius, regardless of the actor's facing direction.
    /// Attach as a child of an <see cref="Actor"/> node.
    /// <para>
    /// Expected scene tree position:
    /// <code>
    /// Actor (Actor.cs)
    ///   └── HearingSensor (HearingSensor.cs)   ← this node
    /// </code>
    /// </para>
    /// </summary>
    public partial class HearingSensor : EntitySensor
    {
        /// <summary>
        /// Detection radius in world-space pixels. All entities within this distance from the
        /// actor's origin are considered heard regardless of facing direction.
        /// </summary>
        [Export] public Single Radius { get; set; } = 128f;


        /// <inheritdoc/>
        public override SensorChannel Channel => SensorChannel.Hearing;


        /// <summary> Attaches a <see cref="CollisionShape2D"/> with a <see cref="CircleShape2D"/> sized to <see cref="Radius"/>. </summary>
        protected override void ConfigureShape()
        {
            CircleShape2D shape = new CircleShape2D();
            shape.Radius = Radius;

            CollisionShape2D collision = new CollisionShape2D();
            collision.Shape = shape;

            AddChild(collision);
        }
    }
}
