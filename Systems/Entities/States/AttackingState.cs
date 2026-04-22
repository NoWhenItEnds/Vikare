using Vikare.Entities.Combat;
using Vikare.Entities.Controllers;
using Vikare.Entities.Interfaces;

namespace Vikare.Entities.States
{
    /// <summary>
    /// Drives the entity through a data-defined melee combo graph described by an <see cref="AttackSequence"/> resource.
    /// One state handles the entire combo; the active step is tracked by <see cref="_currentStepIndex"/> and advanced
    /// by processing <see cref="LightAttackIntent"/> and <see cref="HeavyAttackIntent"/> within the cancel window.
    /// Adding new combos requires only new <c>.tres</c> data assets — this class is never modified (OCP).
    /// </summary>
    /// <remarks>
    /// This state does not write <c>CharacterBody2D.Velocity</c>. Velocity is inherited from the prior state — an
    /// entity attacking whilst sprinting will continue to drift. This is deliberate; the momentum carry-through is
    /// part of the intended feel. To add per-step velocity control (e.g. a lunging heavy attack), add a velocity-curve
    /// field to <see cref="AttackStep"/> and apply it here — no other code needs to change.
    ///
    /// Tick-ordering contract: <see cref="PhysicsProcess"/> accumulates <see cref="_elapsed"/> and fires hit-frame /
    /// duration callbacks; intent-driven step advances happen in <see cref="HandleIntent"/>. Because the state machine
    /// re-delivers the triggering intent to <see cref="HandleIntent"/> immediately after <see cref="Enter"/>, the first
    /// step is chosen in <see cref="HandleIntent"/> rather than <see cref="Enter"/> — this means <see cref="Enter"/>
    /// resets to an unstarted sentinel (<see cref="_currentStepIndex"/> == <c>-1</c>) and plays no animation.
    /// </remarks>
    public sealed class AttackingState : IState
    {
        /// <summary>
        /// Sentinel value meaning no step has been chosen yet (state just entered, awaiting the triggering intent).
        /// </summary>
        private const int NoStepIndex = -1;

        /// <summary>
        /// Combo graph fetched from <see cref="IAttacker"/> on entry; null when the entity does not implement the interface
        /// or has no sequence equipped — in that case the state immediately returns to idle.
        /// </summary>
        private AttackSequence? _sequence;

        /// <summary>
        /// Index into <see cref="_sequence"/>.<see cref="AttackSequence.Steps"/> for the currently active step.
        /// <c>-1</c> (sentinel) between <see cref="Enter"/> and the first <see cref="HandleIntent"/> call.
        /// </summary>
        private int _currentStepIndex = NoStepIndex;

        /// <summary>
        /// Seconds elapsed since the current step began; reset to zero each time a new step is entered.
        /// Compared against <see cref="AttackStep.HitFrameSeconds"/> and <see cref="AttackStep.DurationSeconds"/> each physics tick.
        /// </summary>
        private double _elapsed;

        /// <summary>
        /// Guards <see cref="IAttacker.OnHitFrame"/> so it fires at most once per step activation.
        /// Reset to false whenever <see cref="_currentStepIndex"/> changes.
        /// </summary>
        private bool _hitFrameFired;

        /// <inheritdoc/>
        /// <remarks>
        /// Resets to the unstarted sentinel. No animation is played here — the triggering intent is re-delivered
        /// to <see cref="HandleIntent"/> immediately after this returns, which selects the starting step and plays the animation.
        /// </remarks>
        public void Enter(IStateContext context)
        {
            _sequence = context.As<IAttacker>()?.AttackSequence;
            _currentStepIndex = NoStepIndex;
            _elapsed = 0.0;
            _hitFrameFired = false;
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
        /// When a step is active, the timer advances, the hit frame is fired once, and the step duration is
        /// checked for an idle transition.
        /// </remarks>
        public void PhysicsProcess(IStateContext context, double delta)
        {
            bool sentinelActive = _currentStepIndex == NoStepIndex || _sequence is null;
            bool shouldExit = sentinelActive;

            if (shouldExit)
            {
                context.As<IStateMachineAccess>()?.Machine.ChangeState<IdlingState>();
            }
            else
            {
                bool stepInRange = _currentStepIndex < _sequence!.Steps.Count;
                if (stepInRange)
                {
                    AttackStep step = _sequence.Steps[_currentStepIndex];
                    _elapsed += delta;

                    bool shouldFireHit = !_hitFrameFired && _elapsed >= step.HitFrameSeconds;
                    if (shouldFireHit)
                    {
                        context.As<IAttacker>()?.OnHitFrame(step);
                        _hitFrameFired = true;
                    }

                    bool durationElapsed = _elapsed >= step.DurationSeconds;
                    if (durationElapsed)
                    {
                        context.As<IStateMachineAccess>()?.Machine.ChangeState<IdlingState>();
                    }
                }
            }
        }

        /// <inheritdoc/>
        /// <remarks>
        /// Two cases:
        /// 1. Unstarted (<see cref="_currentStepIndex"/> is sentinel): pick the root step from the intent type.
        ///    If the sequence is null or the root index is <c>-1</c>, immediately transition to idle.
        /// 2. In progress: if the intent type matches a chain direction and the elapsed time is inside the cancel
        ///    window, advance to the successor step. If the successor index is <c>-1</c>, the input is silently
        ///    ignored and the current step runs to completion.
        /// </remarks>
        public void HandleIntent(IStateContext context, IInputIntent intent)
        {
            bool isLight = intent is LightAttackIntent;
            bool isHeavy = intent is HeavyAttackIntent;
            bool isAttackIntent = isLight || isHeavy;

            if (!isAttackIntent)
            {
                // Non-attack intents (e.g. MoveIntent) are silently ignored.
            }
            else if (_currentStepIndex == NoStepIndex)
            {
                AdvanceToRootStep(context, isLight);
            }
            else
            {
                TryChainStep(context, isLight);
            }
        }

        /// <summary>
        /// Selects the root step of the combo based on whether the initiating intent was a light or heavy attack.
        /// Transitions immediately to <see cref="IdlingState"/> when the sequence is null or the relevant root index is <c>-1</c>.
        /// Extracted to keep <see cref="HandleIntent"/> readable and to document the two diverging paths separately.
        /// </summary>
        /// <param name="context">Entity context for capability access and machine access.</param>
        /// <param name="isLight">True when the initiating intent was a <see cref="LightAttackIntent"/>; false for heavy.</param>
        private void AdvanceToRootStep(IStateContext context, bool isLight)
        {
            int rootIndex = NoStepIndex;
            bool sequenceAvailable = _sequence is not null;

            if (sequenceAvailable)
            {
                rootIndex = isLight ? _sequence!.RootOnLight : _sequence!.RootOnHeavy;
            }

            bool rootValid = sequenceAvailable
                && rootIndex != NoStepIndex
                && rootIndex < _sequence!.Steps.Count;

            if (rootValid)
            {
                StartStep(context, rootIndex);
            }
            else
            {
                context.As<IStateMachineAccess>()?.Machine.ChangeState<IdlingState>();
            }
        }

        /// <summary>
        /// Attempts to chain to a successor step when a <see cref="LightAttackIntent"/> or <see cref="HeavyAttackIntent"/>
        /// arrives while a step is already active. Silently ignores the input when the elapsed time is outside the
        /// cancel window or when the successor index is <c>-1</c>.
        /// Extracted to keep <see cref="HandleIntent"/> readable.
        /// </summary>
        /// <param name="context">Entity context for animation and state machine access.</param>
        /// <param name="isLight">True when the chain intent was a <see cref="LightAttackIntent"/>; false for heavy.</param>
        private void TryChainStep(IStateContext context, bool isLight)
        {
            bool stepInRange = _sequence is not null
                && _currentStepIndex < _sequence.Steps.Count;

            if (stepInRange)
            {
                AttackStep current = _sequence!.Steps[_currentStepIndex];
                bool inWindow = _elapsed >= current.CancelWindowStartSeconds
                    && _elapsed <= current.CancelWindowEndSeconds;

                if (inWindow)
                {
                    int nextIndex = isLight ? current.NextOnLight : current.NextOnHeavy;
                    bool chainAvailable = nextIndex != NoStepIndex
                        && nextIndex < _sequence.Steps.Count;

                    if (chainAvailable)
                    {
                        StartStep(context, nextIndex);
                    }
                }
            }
        }

        /// <summary>
        /// Sets <see cref="_currentStepIndex"/> to <paramref name="stepIndex"/>, resets the elapsed timer and hit-frame
        /// guard, and plays the step's animation. Centralised so both the root-entry and chain paths share identical
        /// reset logic with no duplication.
        /// </summary>
        /// <param name="context">Entity context for animation access.</param>
        /// <param name="stepIndex">Index of the step to activate; assumed valid by the caller.</param>
        private void StartStep(IStateContext context, int stepIndex)
        {
            _currentStepIndex = stepIndex;
            _elapsed = 0.0;
            _hitFrameFired = false;

            AttackStep step = _sequence!.Steps[_currentStepIndex];
            context.As<IAnimated>()?.PlayAnimation(step.AnimationName);
        }
    }
}
