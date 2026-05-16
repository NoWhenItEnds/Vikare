using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Vikare.Entities.Sensors;
using Vikare.Managers;

namespace Vikare.Entities.Components
{
    /// <summary> Perception memory component for an actor: records which entities have been detected, when, where, and via which sensor channel, then prunes entries that have not been refreshed. </summary>
    [GlobalClass]
    public partial class MemoryComponent : EntityComponent
    {
        /// <summary> Time in seconds after the last detection event before an entry is pruned. </summary>
        /// <remarks> Entities that remain inside a sensor volume are refreshed every frame and never pruned. </remarks>
        [Export] public Single DecaySeconds { get; set; } = 5f;


        /// <summary> All currently held memory entries, keyed by entity. Entries may be currently detected or ageing towards pruning. </summary>
        private readonly Dictionary<Entity, SensorMemory> _entries = new Dictionary<Entity, SensorMemory>();

        /// <summary> Reusable collection of keys scheduled for removal, allocated once to avoid per-frame heap pressure. </summary>
        private readonly List<Entity> _toRemove = new List<Entity>();

        /// <summary> Per-sensor detected handler, keyed by sensor so each subscription can be disconnected. </summary>
        /// <remarks> Populated in <see cref="OnAddedTo"/> and cleared in <see cref="OnRemovedFrom"/>. </remarks>
        private readonly Dictionary<EntitySensor, EntitySensor.EntityDetectedEventHandler> _detectedHandlers = new Dictionary<EntitySensor, EntitySensor.EntityDetectedEventHandler>();


        /// <inheritdoc/>
        protected internal override void OnAddedTo(Entity entity)
        {
            foreach (EntitySensor sensor in entity.GetChildren().OfType<EntitySensor>())
            {
                SensorChannel channel = sensor.Channel;
                EntitySensor.EntityDetectedEventHandler detectedHandler = detected => OnSensorDetected(detected, channel);

                sensor.EntityDetected += detectedHandler;
                sensor.EntityLost += OnSensorLost;

                _detectedHandlers[sensor] = detectedHandler;
            }
        }


        /// <inheritdoc/>
        protected internal override void OnRemovedFrom(Entity entity)
        {
            foreach (KeyValuePair<EntitySensor, EntitySensor.EntityDetectedEventHandler> pair in _detectedHandlers)
            {
                pair.Key.EntityDetected -= pair.Value;
                pair.Key.EntityLost -= OnSensorLost;
            }

            _detectedHandlers.Clear();
        }


        /// <summary> Records or refreshes a detection of <paramref name="entity"/> at <paramref name="position"/> via <paramref name="channel"/>. </summary>
        /// <param name="entity"> The detected entity. </param>
        /// <param name="position"> World-space position at the moment of detection. </param>
        /// <param name="channel"> The sensor channel reporting the detection. </param>
        /// <remarks> Safe to call every frame while the entity remains inside a sensor. It only updates the existing entry rather than allocating a new one. </remarks>
        public void Remember(Entity entity, Vector2 position, SensorChannel channel)
        {
            DateTime now = GameManager.Instance.CurrentTime;

            if (_entries.TryGetValue(entity, out SensorMemory? existing))
            {
                existing.Refresh(position, now, channel);
            }
            else
            {
                _entries[entity] = new SensorMemory(entity, position, now, channel);
            }
        }


        /// <summary> Marks <paramref name="entity"/> as no longer directly observed; its entry begins ageing and will be pruned unless seen again. </summary>
        /// <param name="entity"> The entity that has left the sensor volume. </param>
        public void Forget(Entity entity)
        {
            if (_entries.TryGetValue(entity, out SensorMemory? entry))
            {
                entry.MarkLost();
            }
        }


        /// <inheritdoc/>
        public override void PhysicsProcess(Double delta)
        {
            DateTime now = GameManager.Instance.CurrentTime;

            _toRemove.Clear();

            foreach (KeyValuePair<Entity, SensorMemory> pair in _entries)
            {
                Boolean entityFreed = !GodotObject.IsInstanceValid(pair.Value.Entity);
                Boolean notCurrentlyDetected = !pair.Value.IsCurrentlyDetected;
                Boolean decayExpired = pair.Value.SecondsSinceDetection(now) >= DecaySeconds;

                if (entityFreed || (notCurrentlyDetected && decayExpired))
                {
                    _toRemove.Add(pair.Key);
                }
            }

            foreach (Entity key in _toRemove)
            {
                _entries.Remove(key);
            }
        }


        /// <summary> Returns the memory entry for <paramref name="entity"/> if one exists. </summary>
        /// <param name="entity"> The entity to look up. </param>
        /// <param name="entry"> The entry if found; null otherwise. </param>
        /// <returns> True if an entry was found. </returns>
        public Boolean TryGet(Entity entity, out SensorMemory? entry) => _entries.TryGetValue(entity, out entry);


        /// <summary> Handles <see cref="EntitySensor.EntityDetected"/> from a subscribed sensor. Writes or refreshes a memory entry using the detected entity's current world position. </summary>
        /// <param name="detected"> The entity that entered the sensor volume. </param>
        /// <param name="channel"> The perceptual channel of the sensor that fired the signal, captured at subscription time. </param>
        private void OnSensorDetected(Entity detected, SensorChannel channel) => Remember(detected, detected.GlobalPosition, channel);


        /// <summary> Handles <see cref="EntitySensor.EntityLost"/> from any subscribed sensor. Marks the entity as no longer directly observed so its memory entry begins ageing. </summary>
        /// <param name="detected"> The entity that exited the sensor volume. </param>
        private void OnSensorLost(Entity detected) => Forget(detected);


        /// <summary> Discards all current memory entries and clears the removal buffer. </summary>
        /// <remarks> Sensor subscriptions are not affected — they persist across tree-exit and re-entry cycles. </remarks>
        public void Clear()
        {
            _entries.Clear();
            _toRemove.Clear();
        }


        /// <summary> Returns a list of valid entities from the current memory.. </summary>
        /// <returns> An array of the currently remembered, and valid, entities. </returns>
        public IEnumerable<Entity> GetEntities()
        {
            HashSet<Entity> entities = new HashSet<Entity>();

            foreach (SensorMemory entry in _entries.Values)
            {
                // This guard defends against a remembered entity that has been freed but not yet removed from the memory entries.
                if (GodotObject.IsInstanceValid(entry.Entity))
                {
                    entities.Add(entry.Entity);
                }
            }

            return entities;
        }
    }
}
