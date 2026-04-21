namespace Vikare.Entities.States.Machines
{
    /// <summary>
    /// State machine for human-type entities; registers the idle, walk, and sprint states with <see cref="IdlingState"/> as the initial state.
    /// </summary>
    public partial class HumanStateMachine : StateMachine
    {
        /// <inheritdoc/>
        protected override void RegisterStates()
        {
            RegisterState<IdlingState>();
            RegisterState<WalkingState>();
            RegisterState<SprintingState>();

            SetInitialState<IdlingState>();
        }
    }
}
