using Vikare.Entities.Combat;
using Vikare.Entities.Controllers;
using Vikare.Entities.Interfaces;

namespace Vikare.Entities.States
{
    /// <summary>
    /// Drives the entity through a data-defined power cast described by a <see cref="PowerDefinition"/> resource.
    /// Supports optional chaining: if the active power's <see cref="PowerDefinition.NextPowers"/> contains the
    /// power named in an incoming <see cref="CastPowerIntent"/>, the cast transitions mid-air to that power.
    /// Adding new powers and chain sequences requires only <c>.tres</c> data assets — this class is never modified (OCP).
    /// </summary>
    /// <remarks>
    /// This state does not write <c>CharacterBody2D.Velocity</c>. Velocity is inherited from the prior state — an
    /// entity casting whilst moving will continue to drift. This is deliberate; the momentum carry-through is the
    /// intended behaviour. To add per-power velocity control (e.g. a backwards-dash cancel), add a velocity-curve
    /// field to <see cref="PowerDefinition"/> and apply it here — no other code needs to change.
    ///
    /// Null-effect contract: <see cref="PowerDefinition.Effect"/> may be null; this represents a purely
    /// animation-based cast with no game-world change (e.g. a cancelled stance animation). The cast proceeds
    /// normally — only the <see cref="PowerEffect.Execute"/> call is skipped. <see cref="FireballPowerEffect"/>
    /// is currently a stub; assign it and replace its <c>Execute</c> body when projectile spawning is ready.
    ///
    /// Tick-ordering note: same as <see cref="AttackingState"/> — <see cref="Enter"/> resets to null/unstarted
    /// sentinel and the triggering <see cref="CastPowerIntent"/> is re-delivered via <see cref="HandleIntent"/>
    /// immediately after <see cref="Enter"/> returns.
    /// </remarks>
    public sealed class CastingPowerState : IState
    {
        /// <summary>
        /// The power currently being cast; null between <see cref="Enter"/> and the first <see cref="HandleIntent"/> call.
        /// Set by <see cref="HandleIntent"/> on each cast initiation or chain.
        /// </summary>
        private PowerDefinition? _power;

        /// <summary>
        /// Seconds elapsed since the current cast began; reset to zero each time a new power is entered or chained.
        /// Compared against <see cref="PowerDefinition.EffectFrameSeconds"/> and <see cref="PowerDefinition.DurationSeconds"/>.
        /// </summary>
        private double _elapsed;

        /// <summary>
        /// Guards <see cref="PowerDefinition.Effect"/>.<see cref="PowerEffect.Execute"/> so it fires at most once per cast activation.
        /// Reset to false whenever <see cref="_power"/> changes.
        /// </summary>
        private bool _effectFired;

        /// <inheritdoc/>
        /// <remarks>
        /// Resets all fields to unstarted sentinel values. No animation is played here — the triggering
        /// <see cref="CastPowerIntent"/> is re-delivered to <see cref="HandleIntent"/> immediately after this returns.
        /// </remarks>
        public void Enter(IStateContext context)
        {
            _power = null;
            _elapsed = 0.0;
            _effectFired = false;
        }

        /// <inheritdoc/>
        public void Exit(IStateContext context)
        {
        }

        /// <inheritdoc/>
        public void Process(IStateContext context, double delta)
        {
        }

        /// <inheritdoc/>
        /// <remarks>
        /// If the state is active but the sentinel is still set (no triggering intent was re-delivered),
        /// self-transition to <see cref="IdlingState"/>. This defends against direct <c>ChangeState&lt;T&gt;()</c>
        /// entries that bypass the transition table's intent re-delivery.
        ///
        /// When a power is active, advances the timer, fires the effect once at <see cref="PowerDefinition.EffectFrameSeconds"/>,
        /// and transitions to idle when the full <see cref="PowerDefinition.DurationSeconds"/> elapses.
        /// A null <see cref="PowerDefinition.Effect"/> means the effect step is silently skipped.
        /// </remarks>
        public void PhysicsProcess(IStateContext context, double delta)
        {
            bool shouldExit = _power is null;

            if (shouldExit)
            {
                context.As<IStateMachineAccess>()?.Machine.ChangeState<IdlingState>();
            }
            else
            {
                _elapsed += delta;

                bool shouldFireEffect = !_effectFired && _elapsed >= _power!.EffectFrameSeconds;
                if (shouldFireEffect)
                {
                    _power.Effect?.Execute(context);
                    _effectFired = true;
                }

                bool durationElapsed = _elapsed >= _power.DurationSeconds;
                if (durationElapsed)
                {
                    context.As<IStateMachineAccess>()?.Machine.ChangeState<IdlingState>();
                }
            }
        }

        /// <inheritdoc/>
        /// <remarks>
        /// Two cases:
        /// 1. Unstarted (<see cref="_power"/> is null): the intent must be a <see cref="CastPowerIntent"/> with a non-null
        ///    <see cref="CastPowerIntent.Power"/>. If not, immediately transition to idle (defensive guard against table mis-configuration).
        /// 2. In progress: if the intent is a <see cref="CastPowerIntent"/> and the elapsed time is inside the cancel
        ///    window and the intent's power is listed in <see cref="PowerDefinition.NextPowers"/>, chain to that power.
        ///    Otherwise the input is silently ignored and the current cast runs to completion.
        /// </remarks>
        public void HandleIntent(IStateContext context, IInputIntent intent)
        {
            if (intent is CastPowerIntent castIntent)
            {
                if (_power is null)
                {
                    StartCast(context, castIntent.Power);
                }
                else
                {
                    TryChainCast(context, castIntent);
                }
            }
            // Non-cast intents are silently ignored.
        }

        /// <summary>
        /// Initiates a cast from the unstarted sentinel state. Transitions to <see cref="IdlingState"/> when
        /// <paramref name="power"/> is null (the transition table predicate should prevent this, but a second guard
        /// here protects against future table mis-configuration).
        /// Extracted to keep <see cref="HandleIntent"/> readable.
        /// </summary>
        /// <param name="context">Entity context for animation and state machine access.</param>
        /// <param name="power">The power definition to cast; null causes an immediate idle transition.</param>
        private void StartCast(IStateContext context, PowerDefinition? power)
        {
            bool powerValid = power is not null;
            if (powerValid)
            {
                BeginPower(context, power!);
            }
            else
            {
                context.As<IStateMachineAccess>()?.Machine.ChangeState<IdlingState>();
            }
        }

        /// <summary>
        /// Attempts to chain from the active cast to a successor power when a <see cref="CastPowerIntent"/>
        /// arrives within the cancel window and the intent's power is listed in <see cref="PowerDefinition.NextPowers"/>.
        /// Silently ignores the input when outside the window or when the power is not a valid chain target.
        /// Extracted to keep <see cref="HandleIntent"/> readable.
        /// </summary>
        /// <param name="context">Entity context for animation and state machine access.</param>
        /// <param name="intent">The incoming cast intent; its <see cref="CastPowerIntent.Power"/> is the chain candidate.</param>
        private void TryChainCast(IStateContext context, CastPowerIntent intent)
        {
            bool inWindow = _elapsed >= _power!.CancelWindowStartSeconds
                && _elapsed <= _power.CancelWindowEndSeconds;

            bool chainPowerValid = intent.Power is not null;
            bool chainAvailable = inWindow && chainPowerValid && _power.NextPowers.Contains(intent.Power!);

            if (chainAvailable)
            {
                BeginPower(context, intent.Power!);
            }
        }

        /// <summary>
        /// Assigns <paramref name="power"/> as the current cast, resets the elapsed timer and effect guard,
        /// and plays the power's animation. Centralised so both the initial-cast and chain paths share identical
        /// reset logic with no duplication.
        /// </summary>
        /// <param name="context">Entity context for animation access.</param>
        /// <param name="power">The power definition to begin; assumed non-null by the caller.</param>
        private void BeginPower(IStateContext context, PowerDefinition power)
        {
            _power = power;
            _elapsed = 0.0;
            _effectFired = false;
            context.As<IAnimated>()?.PlayAnimation(_power.AnimationName);
        }
    }
}
