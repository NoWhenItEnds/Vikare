using Vikare.Entities.Intents;

namespace Vikare.Entities.Interfaces
{
    /// <summary>
    /// Contract for a single state in the entity finite state machine. Concrete states represent one
    /// distinct behavioural mode (idle, walking, blocking, ability execution, etc.).
    /// States are plain C# classes that receive the owning <see cref="Vikare.Entities.Actor"/> directly.
    /// </summary>
    public interface IState
    {
        /// <summary>
        /// Called once when this state becomes active. Play the entry animation and reset local fields.
        /// </summary>
        /// <param name="actor">The actor entering this state.</param>
        void Enter(Actor actor);

        /// <summary>
        /// Called once when this state is deactivated. Clean up any transient effects.
        /// </summary>
        /// <param name="actor">The actor exiting this state.</param>
        void Exit(Actor actor);

        /// <summary>
        /// Called every visual frame while this state is active. Use for visuals-only updates.
        /// </summary>
        /// <param name="actor">The owning actor.</param>
        /// <param name="delta">Elapsed time since the last visual frame, in seconds.</param>
        void Process(Actor actor, double delta);

        /// <summary>
        /// Called every physics tick while this state is active. Use for velocity writes and timers.
        /// </summary>
        /// <param name="actor">The owning actor.</param>
        /// <param name="delta">Elapsed time since the last physics tick, in seconds.</param>
        void PhysicsProcess(Actor actor, double delta);

        /// <summary>
        /// Called when the controller produces an <see cref="ActionIntent"/>. Unknown intent types must
        /// be silently ignored.
        /// </summary>
        /// <param name="actor">The owning actor.</param>
        /// <param name="intent">The controller's intent; use pattern matching to check for specific types.</param>
        void HandleIntent(Actor actor, ActionIntent intent);
    }
}
