using Vikare.Entities.Intents;

namespace Vikare.Entities.States.Machines
{
    /// <summary>
    /// State machine for human-type entities; registers the idle, walk, sprint, block, dodge, attack, and power-casting
    /// states with <see cref="IdlingState"/> as the initial state.
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
            RegisterState<AttackingState>();
            RegisterState<CastingPowerState>();

            SetInitialState<IdlingState>();
        }

        /// <inheritdoc/>
        /// <remarks>
        /// Attack and cast transitions are available from idle, walk, and sprint — but not from block or dodge,
        /// and not from within attack or cast themselves (self-exit to idle handles the tail). Cross-chaining between
        /// attack and cast, and cancel-into-block/dodge from attack/cast, are deferred to a future iteration.
        /// </remarks>
        protected override void RegisterTransitions()
        {
            When<IdlingState>().On<MoveIntent>(i => i.Direction != Godot.Vector2.Zero).Transition<WalkingState>();
            When<IdlingState>().On<BlockIntent>(i => i.IsBlocking).Transition<BlockingState>();
            When<IdlingState>().On<DodgeIntent>(_ => true).Transition<DodgingState>();
            When<IdlingState>().On<LightAttackIntent>(_ => true).Transition<AttackingState>();
            When<IdlingState>().On<HeavyAttackIntent>(_ => true).Transition<AttackingState>();
            When<IdlingState>().On<CastPowerIntent>(i => i.Power != null).Transition<CastingPowerState>();

            When<WalkingState>().On<MoveIntent>(i => i.Direction == Godot.Vector2.Zero).Transition<IdlingState>();
            When<WalkingState>().On<SprintIntent>(i => i.IsSprinting).Transition<SprintingState>();
            When<WalkingState>().On<BlockIntent>(i => i.IsBlocking).Transition<BlockingState>();
            When<WalkingState>().On<DodgeIntent>(_ => true).Transition<DodgingState>();
            When<WalkingState>().On<LightAttackIntent>(_ => true).Transition<AttackingState>();
            When<WalkingState>().On<HeavyAttackIntent>(_ => true).Transition<AttackingState>();
            When<WalkingState>().On<CastPowerIntent>(i => i.Power != null).Transition<CastingPowerState>();

            When<SprintingState>().On<MoveIntent>(i => i.Direction == Godot.Vector2.Zero).Transition<IdlingState>();
            When<SprintingState>().On<SprintIntent>(i => !i.IsSprinting).Transition<WalkingState>();
            When<SprintingState>().On<BlockIntent>(i => i.IsBlocking).Transition<BlockingState>();
            When<SprintingState>().On<DodgeIntent>(_ => true).Transition<DodgingState>();
            When<SprintingState>().On<LightAttackIntent>(_ => true).Transition<AttackingState>();
            When<SprintingState>().On<HeavyAttackIntent>(_ => true).Transition<AttackingState>();
            When<SprintingState>().On<CastPowerIntent>(i => i.Power != null).Transition<CastingPowerState>();

            When<BlockingState>().On<BlockIntent>(i => !i.IsBlocking).Transition<IdlingState>();
        }
    }
}
