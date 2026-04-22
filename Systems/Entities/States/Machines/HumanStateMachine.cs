using Vikare.Entities.Controllers;

namespace Vikare.Entities.States.Machines
{
    /// <summary>
    /// State machine for human-type entities; registers the idle, walk, sprint, block, and dodge states with <see cref="IdlingState"/> as the initial state.
    /// </summary>
    public partial class HumanStateMachine : StateMachine
    {
        /// <inheritdoc/>
        protected override void RegisterStates()
        {
            RegisterState<IdlingState>();
            RegisterState<WalkingState>();
            RegisterState<SprintingState>();
            RegisterState<BlockingState>();
            RegisterState<DodgingState>();

            SetInitialState<IdlingState>();
        }

        /// <inheritdoc/>
        protected override void RegisterTransitions()
        {
            When<IdlingState>().On<MoveIntent>(i => i.Direction != Godot.Vector2.Zero).Transition<WalkingState>();
            When<IdlingState>().On<BlockIntent>(i => i.IsBlocking).Transition<BlockingState>();
            When<IdlingState>().On<DodgeIntent>(_ => true).Transition<DodgingState>();

            When<WalkingState>().On<MoveIntent>(i => i.Direction == Godot.Vector2.Zero).Transition<IdlingState>();
            When<WalkingState>().On<SprintIntent>(i => i.IsSprinting).Transition<SprintingState>();
            When<WalkingState>().On<BlockIntent>(i => i.IsBlocking).Transition<BlockingState>();
            When<WalkingState>().On<DodgeIntent>(_ => true).Transition<DodgingState>();

            When<SprintingState>().On<MoveIntent>(i => i.Direction == Godot.Vector2.Zero).Transition<IdlingState>();
            When<SprintingState>().On<SprintIntent>(i => !i.IsSprinting).Transition<WalkingState>();
            When<SprintingState>().On<BlockIntent>(i => i.IsBlocking).Transition<BlockingState>();
            When<SprintingState>().On<DodgeIntent>(_ => true).Transition<DodgingState>();

            When<BlockingState>().On<BlockIntent>(i => !i.IsBlocking).Transition<IdlingState>();
        }
    }
}
