using Godot;
using Vikare.Entities.Combat;
using Vikare.Entities.Intents;
using Vikare.Entities.Interfaces;

namespace Vikare.Entities.States
{
    /// <summary>
    /// Drives any ability — melee combos, power casts, or a cross-chain mix — through a data-defined
    /// graph of <see cref="AbilityStep"/> resources held in an <see cref="AbilitySequence"/>.
    /// </summary>
    /// <remarks>
    /// Entry contract: <see cref="Enter"/> resets all fields. The state machine re-delivers the
    /// triggering <see cref="AbilityIntent"/> to <see cref="HandleIntent"/> immediately after
    /// <see cref="Enter"/> returns, which selects the starting step and plays its animation.
    ///
    /// Cancel-window contract: incoming <see cref="AbilityIntent"/> messages are only accepted between
    /// <see cref="AbilityStep.CancelWindowStartSeconds"/> and <see cref="AbilityStep.CancelWindowEndSeconds"/>.
    /// If the active step's <see cref="AbilityStep.Transitions"/> map contains the intent's key, the
    /// state advances to the mapped successor; otherwise the input is discarded.
    ///
    /// Adding new combos or cross-chain transitions requires only new <c>.tres</c> data assets (OCP).
    /// </remarks>
    public sealed class AbilityState : IState
    {
        /// <summary>Sentinel indicating no step is active; -1 is never a valid step index.</summary>
        private const int NoStepIndex = -1;

        /// <summary>
        /// Ability graph fetched from the actor on entry. Null when the actor has no sequence
        /// equipped — the state immediately transitions to <see cref="IdlingState"/> in that case.
        /// </summary>
        private AbilitySequence? _sequence;

        /// <summary>Index of the active step in <see cref="_sequence"/>; <see cref="NoStepIndex"/> when unstarted.</summary>
        private int _currentStepIndex = NoStepIndex;

        /// <summary>
        /// Seconds elapsed since the current step began. Compared each physics tick against effect,
        /// cancel-window, and duration thresholds. Reset to zero on each new step.
        /// </summary>
        private double _elapsed;

        /// <summary>Prevents <see cref="AbilityStep.Effect"/> from firing more than once per step activation.</summary>
        private bool _effectFired;

        /// <summary>Resets all fields to their unstarted sentinel values.</summary>
        /// <remarks>
        /// No animation is played here — the triggering <see cref="AbilityIntent"/> is re-delivered to
        /// <see cref="HandleIntent"/> by the state machine immediately after this returns.
        /// </remarks>
        public void Enter(Actor actor)
        {
            _sequence = actor.AbilitySequence;
            _currentStepIndex = NoStepIndex;
            _elapsed = 0.0;
            _effectFired = false;
        }

        /// <inheritdoc/>
        public void Exit(Actor actor)
        {
        }

        /// <inheritdoc/>
        public void Process(Actor actor, double delta)
        {
        }

        /// <summary>
        /// When a step is active: advances the timer, fires the effect once at the effect frame,
        /// and transitions to idle when the full duration elapses.
        /// When the sentinel is still active (no step chosen), logs a warning and transitions to idle.
        /// </summary>
        public void PhysicsProcess(Actor actor, double delta)
        {
            bool sentinelActive = _currentStepIndex == NoStepIndex || _sequence is null;

            if (sentinelActive)
            {
                GD.PushWarning(
                    "[AbilityState] PhysicsProcess reached with sentinel active (no step chosen). " +
                    "AbilityState should only be entered via an AbilityIntent re-delivery. " +
                    "Check that ChangeState<AbilityState>() is not called directly.");
                actor.Machine.ChangeState<IdlingState>();
            }
            else
            {
                bool stepInRange = _currentStepIndex < _sequence!.Steps.Count;
                if (stepInRange)
                {
                    AbilityStep step = _sequence.Steps[_currentStepIndex];
                    _elapsed += delta;

                    bool shouldFireEffect = !_effectFired && _elapsed >= step.EffectFrameSeconds;
                    if (shouldFireEffect)
                    {
                        step.Effect?.Execute(actor);
                        _effectFired = true;
                    }

                    bool durationElapsed = _elapsed >= step.DurationSeconds;
                    if (durationElapsed)
                    {
                        actor.Machine.ChangeState<IdlingState>();
                    }
                }
            }
        }

        /// <summary>
        /// When unstarted: looks up the intent's key in <see cref="AbilitySequence.EntrySteps"/> to
        /// find the root step; transitions to idle if the lookup fails.
        /// When a step is active: attempts to chain to a successor if the elapsed time is inside the
        /// cancel window and the active step's transitions map contains the key.
        /// Non-<see cref="AbilityIntent"/> intents are silently ignored.
        /// </summary>
        public void HandleIntent(Actor actor, ActionIntent intent)
        {
            if (intent is AbilityIntent abilityIntent)
            {
                bool isUnstarted = _currentStepIndex == NoStepIndex;
                if (isUnstarted)
                {
                    AdvanceToRootStep(actor, abilityIntent.Key);
                }
                else
                {
                    TryChainStep(actor, abilityIntent.Key);
                }
            }
        }

        /// <summary>
        /// Looks up <paramref name="key"/> in <see cref="AbilitySequence.EntrySteps"/> and starts the
        /// root step. Transitions to <see cref="IdlingState"/> when the sequence is null, the key has
        /// no entry, or the index is out of range.
        /// </summary>
        private void AdvanceToRootStep(Actor actor, AbilityKey key)
        {
            bool sequenceAvailable = _sequence is not null;
            int keyInt = (int)key;
            // rootIndex initialised to NoStepIndex so the rootValid guard is safe when
            // sequenceAvailable is false and TryGetValue never runs.
            int rootIndex = NoStepIndex;
            bool entryExists = sequenceAvailable
                && _sequence!.EntrySteps.TryGetValue(keyInt, out rootIndex);

            bool rootValid = entryExists
                && rootIndex != NoStepIndex
                && rootIndex < _sequence!.Steps.Count;

            if (rootValid)
            {
                StartStep(actor, rootIndex);
            }
            else
            {
                actor.Machine.ChangeState<IdlingState>();
            }
        }

        /// <summary>
        /// Attempts to chain to a successor step when a cancel-window <see cref="AbilityIntent"/>
        /// arrives and the active step's transitions map contains <paramref name="key"/>.
        /// Silently discards the input if any condition is not met.
        /// </summary>
        private void TryChainStep(Actor actor, AbilityKey key)
        {
            bool sequenceValid = _sequence is not null
                && _currentStepIndex < _sequence.Steps.Count;

            if (sequenceValid)
            {
                AbilityStep current = _sequence!.Steps[_currentStepIndex];
                bool inWindow = _elapsed >= current.CancelWindowStartSeconds
                    && _elapsed <= current.CancelWindowEndSeconds;

                if (inWindow)
                {
                    int keyInt = (int)key;
                    bool chainExists = current.Transitions.TryGetValue(keyInt, out int nextIndex);
                    if (chainExists)
                    {
                        bool chainValid = nextIndex != NoStepIndex
                            && nextIndex < _sequence.Steps.Count;

                        if (chainValid)
                        {
                            StartStep(actor, nextIndex);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Sets <see cref="_currentStepIndex"/>, resets the timer and effect guard, and plays the
        /// step's animation. Shared by the root-entry and chain paths (DRY).
        /// </summary>
        private void StartStep(Actor actor, int stepIndex)
        {
            _currentStepIndex = stepIndex;
            _elapsed = 0.0;
            _effectFired = false;

            AbilityStep step = _sequence!.Steps[_currentStepIndex];
            actor.PlayAnimation(step.AnimationName);
        }
    }
}
