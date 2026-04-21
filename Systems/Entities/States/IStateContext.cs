namespace Vikare.Entities.States
{
    /// <summary>
    /// Window into the entity passed to every state method; exposes capabilities via <c>As&lt;T&gt;()</c> rather than the full concrete type.
    /// </summary>
    public interface IStateContext
    {
        /// <summary>
        /// Returns the entity cast to <typeparamref name="TCapability"/>, or null if the entity does not implement it.
        /// </summary>
        /// <typeparam name="TCapability">The capability interface to retrieve (e.g. <c>IMovable</c>).</typeparam>
        /// <returns>The entity cast to <typeparamref name="TCapability"/>, or null if not supported.</returns>
        TCapability? As<TCapability>() where TCapability : class;
    }
}
