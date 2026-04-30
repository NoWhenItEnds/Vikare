using System;
using Godot;

namespace Vikare.Entities.Components
{
    /// <summary> A sub-component belonging to an entity. Provides additional information and statistics. </summary>
    [GlobalClass]
    public abstract partial class EntityComponent : Resource, IEquatable<EntityComponent>
    {
        /// <inheritdoc/>
        public override Int32 GetHashCode() => GetType().GetHashCode();


        /// <inheritdoc/>
        public Boolean Equals(EntityComponent? other) => other != null && GetType() == other.GetType();
    }
}
