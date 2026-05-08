namespace Vikare.Entities.Sensors
{
    /// <summary>
    /// Identifies which perceptual channel last detected an entity, stored inside a
    /// <see cref="Vikare.Entities.Memory.MemoryEntry"/>. Declared as an enum so that new channels
    /// (e.g. <c>Smell</c>, <c>Tremorsense</c>) can be added by appending a new member without
    /// touching existing sensor or memory code — satisfying the open/closed principle for the
    /// common case of additive extension.
    /// </summary>
    public enum SensorChannel
    {
        /// <summary> The entity was last detected by a line-of-sight cone sensor. </summary>
        Sight,

        /// <summary> The entity was last detected by an omnidirectional hearing sensor. </summary>
        Hearing,
    }
}
