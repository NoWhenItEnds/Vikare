using Godot;
using Vikare.Entities.Interfaces;

namespace Vikare.Entities.States
{
    /// <summary>
    /// Concrete <see cref="IStateContext"/> that wraps a Godot node and fulfils capability queries via a C# <c>as</c> cast.
    /// </summary>
    public sealed class StateContext : IStateContext
    {
        /// <summary>
        /// The entity node being wrapped; capability queries succeed only for interfaces the node actually implements.
        /// </summary>
        private readonly Node _entity;

        /// <summary>
        /// Initialises a new <see cref="StateContext"/> wrapping the supplied entity node.
        /// </summary>
        /// <param name="entity">The entity node states will operate on. Must not be null.</param>
        public StateContext(Node entity)
        {
            _entity = entity;
        }

        /// <inheritdoc/>
        public TCapability? As<TCapability>() where TCapability : class
        {
            return _entity as TCapability;
        }
    }
}
