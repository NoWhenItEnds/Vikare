using System;
using Godot;

namespace Vikare.Entities.Components
{
    /// <summary> A sub-component belonging to an entity. Provides additional information and statistics. </summary>
    [GlobalClass]
    public abstract partial class EntityComponent : Resource, IEquatable<EntityComponent>
    {
        /// <summary> Invoked once after this component has been added to <paramref name="entity"/>. </summary>
        /// <param name="entity"> The entity that has just taken ownership of this component. </param>
        protected internal virtual void OnAddedTo(Entity entity) { }

        /// <summary> Invoked once after this component has been removed from <paramref name="entity"/>. </summary>
        /// <param name="entity"> The entity that has just released this component. </param>
        protected internal virtual void OnRemovedFrom(Entity entity) { }


        /// <summary> Update the component by integrating it to it's associated entity's physics process loop. </summary>
        /// <param name="delta"> The time passed since the previous frame. </param>
        public virtual void PhysicsProcess(Double delta) { }


        /// <inheritdoc/>
        public override Int32 GetHashCode() => GetType().GetHashCode();


        /// <inheritdoc/>
        public Boolean Equals(EntityComponent? other) => other != null && GetType() == other.GetType();
    }
}
