namespace Vikare.Entities.Interfaces
{
    /// <summary>
    /// Contract for a single state in the entity finite state machine. Each concrete state represents one behavioural mode.
    /// </summary>
    public interface IState
    {
        /// <summary>
        /// Called once when this state becomes active. Use to play the entry animation and reset local state.
        /// </summary>
        /// <param name="context">Entity context exposing capabilities available during entry.</param>
        void Enter(IStateContext context);

        /// <summary>
        /// Called once when this state is deactivated. Use to clean up transient effects that must not persist.
        /// </summary>
        /// <param name="context">Entity context exposing capabilities available during exit.</param>
        void Exit(IStateContext context);

        /// <summary>
        /// Called every visual frame while active — equivalent to Godot's <c>_Process</c>.
        /// </summary>
        /// <param name="context">Entity context for this frame.</param>
        /// <param name="delta">Elapsed time since the last frame, in seconds.</param>
        void Process(IStateContext context, double delta);

        /// <summary>
        /// Called every physics tick while active — equivalent to Godot's <c>_PhysicsProcess</c>. Use for velocity updates.
        /// </summary>
        /// <param name="context">Entity context for this physics tick.</param>
        /// <param name="delta">Elapsed time since the last physics tick, in seconds.</param>
        void PhysicsProcess(IStateContext context, double delta);

        /// <summary>
        /// Called when the controller produces an <see cref="IInputIntent"/>. Unknown intent types must be silently ignored.
        /// </summary>
        /// <param name="context">Entity context at the time of the input event.</param>
        /// <param name="intent">The controller's intent; use pattern matching to check for specific types.</param>
        void HandleIntent(IStateContext context, IInputIntent intent);
    }
}
