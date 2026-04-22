using Vikare.Entities.States;

namespace Vikare.Entities.Interfaces
{
    /// <summary>
    /// Capability contract that exposes a reference to the entity's own <see cref="StateMachine"/> so states can request transitions without walking the scene tree.
    /// </summary>
    public interface IStateMachineAccess
    {
        /// <summary>
        /// The state machine attached to this entity; use exclusively to call <see cref="StateMachine.ChangeState{TState}"/>.
        /// </summary>
        StateMachine Machine { get; }
    }
}
