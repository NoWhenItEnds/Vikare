using Godot;

namespace Vikare.Entities.States.Machines
{
    /// <summary>
    /// State machine for human-type entities. Registers idle, walk, sprint, block, dodge, and ability states.
    /// </summary>
    /// <remarks>
    /// Ability activation is permitted from idle, walk, and sprint only — not from block or dodge.
    /// Cancel-window combo links are handled internally by <see cref="AbilityState"/>; no transition table
    /// entries are needed here for those paths.
    /// </remarks>
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
            RegisterState<AbilityState>();

            SetInitialState<IdlingState>();
        }

        /// <inheritdoc/>
        protected override void RegisterTransitions()
        {
            When<IdlingState>().On<WalkIntent>(i => i.Direction != Vector2.Zero).Transition<WalkingState>();
            When<IdlingState>().On<SprintIntent>(i => i.Direction != Vector2.Zero).Transition<SprintingState>();
            When<IdlingState>().On<BlockIntent>(_ => true).Transition<BlockingState>();
            When<IdlingState>().On<DodgeIntent>(_ => true).Transition<DodgingState>();
            //When<IdlingState>().On<AbilityIntent>(i => _actor!.Abilities.Contains(i.Ability)).Transition<AbilityState>();

            When<WalkingState>().On<WalkIntent>(i => i.Direction == Vector2.Zero).Transition<IdlingState>();
            When<WalkingState>().On<SprintIntent>(i => i.Direction != Vector2.Zero).Transition<SprintingState>();
            When<WalkingState>().On<BlockIntent>(_ => true).Transition<BlockingState>();
            When<WalkingState>().On<DodgeIntent>(_ => true).Transition<DodgingState>();
            //When<WalkingState>().On<AbilityIntent>(i => _actor!.Abilities.Contains(i.Ability)).Transition<AbilityState>();

            When<SprintingState>().On<WalkIntent>(i => i.Direction == Vector2.Zero).Transition<IdlingState>();
            When<SprintingState>().On<WalkIntent>(i => i.Direction != Vector2.Zero).Transition<WalkingState>();
            When<SprintingState>().On<BlockIntent>(_ => true).Transition<BlockingState>();
            When<SprintingState>().On<DodgeIntent>(_ => true).Transition<DodgingState>();
            //When<SprintingState>().On<AbilityIntent>(i => _actor!.Abilities.Contains(i.Ability)).Transition<AbilityState>();

            When<BlockingState>().On<BlockIntent>(_ => true).Transition<IdlingState>();
        }
    }
}
