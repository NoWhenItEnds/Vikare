using System;
using Godot;

namespace Vikare.Entities.Sensors
{
    /// <summary>
    /// An immutable snapshot of a single perceived entity, held inside <see cref="EntityMemory"/>.
    /// <para>
    /// Invariant: <see cref="LastKnownPosition"/> reflects the entity's world position at the
    /// moment of the most recent detection event and is never updated while the entity is
    /// unobserved — callers must treat it as a stale estimate once the entity has left the sensor.
    /// </para>
    /// </summary>
    public sealed class SensorMemory
    {
        /// <summary> The perceived entity. Never null after construction. </summary>
        public Entity Entity { get; }

        /// <summary>
        /// World-space position captured at the most recent detection event.
        /// Not updated while the entity is outside the sensor's range.
        /// </summary>
        public Vector2 LastKnownPosition { get; private set; }

        /// <summary>
        /// Game-world timestamp of the most recent detection event, sourced from
        /// <see cref="Vikare.Managers.GameManager.CurrentTime"/>. Only meaningful relative to
        /// other timestamps produced by the same game clock.
        /// </summary>
        public DateTime LastDetectedAt { get; private set; }

        /// <summary> The sensor channel that last confirmed this entity's presence. </summary>
        public SensorChannel LastChannel { get; private set; }

        /// <summary>
        /// Whether the entity is currently inside an active sensor's detection volume.
        /// When false the entry is ageing towards pruning.
        /// </summary>
        public Boolean IsCurrentlyDetected { get; private set; }


        /// <summary>
        /// Initialises a new entry for <paramref name="entity"/> detected via <paramref name="channel"/>
        /// at <paramref name="position"/>, stamped with the current game-world time.
        /// </summary>
        /// <param name="entity"> The entity that was detected. </param>
        /// <param name="position"> World-space position at the moment of detection. </param>
        /// <param name="currentTime"> The game-world clock value at detection, from <see cref="Vikare.Managers.GameManager.CurrentTime"/>. </param>
        /// <param name="channel"> The sensor channel that produced the detection. </param>
        public SensorMemory(Entity entity, Vector2 position, DateTime currentTime, SensorChannel channel)
        {
            Entity = entity;
            LastKnownPosition = position;
            LastDetectedAt = currentTime;
            LastChannel = channel;
            IsCurrentlyDetected = true;
        }


        /// <summary>
        /// Refreshes all mutable fields with a new detection reading; called each frame while the
        /// entity remains inside a sensor volume.
        /// </summary>
        /// <param name="position"> Updated world-space position. </param>
        /// <param name="currentTime"> The game-world clock value at this refresh, from <see cref="Vikare.Managers.GameManager.CurrentTime"/>. </param>
        /// <param name="channel"> The sensor channel reporting the refresh. </param>
        public void Refresh(Vector2 position, DateTime currentTime, SensorChannel channel)
        {
            LastKnownPosition = position;
            LastDetectedAt = currentTime;
            LastChannel = channel;
            IsCurrentlyDetected = true;
        }


        /// <summary>
        /// Marks the entity as no longer directly observed; the entry begins ageing towards pruning.
        /// <see cref="LastKnownPosition"/> is intentionally not updated — it retains the last
        /// confirmed position so the controller has a stale estimate to act on.
        /// </summary>
        public void MarkLost()
        {
            IsCurrentlyDetected = false;
        }


        /// <summary>
        /// Returns the number of seconds that have elapsed since this entity was last detected,
        /// relative to the supplied game-world clock value.
        /// </summary>
        /// <param name="currentTime"> The game-world clock value to measure against, from <see cref="Vikare.Managers.GameManager.CurrentTime"/>. </param>
        /// <returns>
        /// Seconds since the most recent detection event. A negative result indicates a clock
        /// inconsistency (e.g. a save/load reset) and is returned as-is for the caller to handle.
        /// </returns>
        public Double SecondsSinceDetection(DateTime currentTime)
        {
            return (currentTime - LastDetectedAt).TotalSeconds;
        }
    }
}
