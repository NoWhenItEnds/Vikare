using System;
using System.Collections.Generic;
using Godot;
using Vikare.Entities.Intents;
using Vikare.Entities.Interfaces;

namespace Vikare.Entities.States
{
    /// <summary>
    /// Node-based finite state machine base class. Attach as a child of an <see cref="Vikare.Entities.Actor"/>.
    /// Subclass it, override <see cref="RegisterStates"/> to call <see cref="RegisterState{TState}"/> and
    /// <see cref="SetInitialState{TState}"/>, then override <see cref="RegisterTransitions"/> to declare
    /// transitions via the fluent <see cref="When{TSource}"/> builder.
    /// </summary>
    /// <remarks>
    /// Tick ordering: Godot processes parent nodes before children, so <c>Entity._PhysicsProcess</c>
    /// (which calls <c>MoveAndSlide</c>) runs before the state machine's <c>_PhysicsProcess</c>.
    /// Velocity written by the active state at tick N is consumed at tick N+1.
    ///
    /// Intent routing: <see cref="HandleIntent"/> consults the transition table first, fires any matching
    /// transition, then delivers the intent to the (now-current) state so it always receives the intent
    /// that triggered its entry.
    /// </remarks>
    public abstract partial class StateMachine : Node
    {
        /// <summary>
        /// A row in the transition table: source state type, intent predicate, and target state type.
        /// </summary>
        internal readonly struct TransitionEntry
        {
            /// <summary>Source state type; matched against <c>_currentState.GetType()</c>.</summary>
            public Type SourceType { get; }

            /// <summary>Returns true when the intent satisfies the condition for this transition.</summary>
            public Func<ActionIntent, bool> Predicate { get; }

            /// <summary>State type to enter when the predicate matches.</summary>
            public Type TargetType { get; }

            /// <summary>Initialises all three fields.</summary>
            public TransitionEntry(Type sourceType, Func<ActionIntent, bool> predicate, Type targetType)
            {
                SourceType = sourceType;
                Predicate = predicate;
                TargetType = targetType;
            }
        }

        /// <summary>
        /// Registered transitions consulted in order on each <see cref="HandleIntent"/> call.
        /// The first matching entry wins.
        /// </summary>
        private readonly List<TransitionEntry> _transitions = new();

        /// <summary>State the machine enters after <c>_Ready</c>; set by <see cref="SetInitialState{TState}"/>.</summary>
        private Type? _initialStateType;

        /// <summary>All registered states keyed by concrete type.</summary>
        private readonly Dictionary<Type, IState> _states = new();

        /// <summary>Currently active state; null only before <c>_Ready</c> completes.</summary>
        private IState? _currentState;

        /// <summary>
        /// The actor that owns this machine; cached from the parent node in <c>_Ready</c>.
        /// </summary>
        private Actor? _actor;

        /// <summary>
        /// Resolves the parent actor, registers states and transitions, validates the table,
        /// then enters the initial state.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the initial state was not set, or the transition table references an unregistered type.
        /// </exception>
        public override void _Ready()
        {
            _actor = GetParent<Actor>();

            RegisterStates();

            bool initialStateMissing = _initialStateType == null;
            if (initialStateMissing)
            {
                throw new InvalidOperationException(
                    $"{GetType().Name} did not call SetInitialState<T>() inside RegisterStates(). " +
                    "Every state machine must define an initial state.");
            }

            RegisterTransitions();
            ValidateTransitionTable();

            EnterState(_states[_initialStateType!]);
        }

        /// <summary>Forwards the visual-frame tick to the active state.</summary>
        /// <param name="delta">Elapsed time since the last visual frame, in seconds.</param>
        public override void _Process(double delta)
        {
            _currentState?.Process(_actor!, delta);
        }

        /// <summary>Forwards the physics tick to the active state.</summary>
        /// <param name="delta">Elapsed time since the last physics tick, in seconds.</param>
        public override void _PhysicsProcess(double delta)
        {
            _currentState?.PhysicsProcess(_actor!, delta);
        }

        /// <summary>
        /// Consults the transition table, fires the first matching transition, then delivers the intent
        /// to the now-current state. Silently dropped if the machine has not yet initialised.
        /// </summary>
        /// <param name="intent">The controller's intent.</param>
        public void HandleIntent(ActionIntent intent)
        {
            bool isReady = _currentState != null && _actor != null;
            if (isReady)
            {
                Type? matchedTarget = FindTransitionTarget(intent);
                bool transitionFound = matchedTarget != null;
                if (transitionFound)
                {
                    ChangeState(matchedTarget!);
                }

                _currentState!.HandleIntent(_actor!, intent);
            }
        }

        /// <summary>
        /// Transitions to <typeparamref name="TState"/>. No-op if already in that state.
        /// </summary>
        /// <remarks>Exit and Enter complete synchronously. Do not call from within Enter or Exit.</remarks>
        /// <typeparam name="TState">Target state; must have been registered via <see cref="RegisterState{TState}"/>.</typeparam>
        public void ChangeState<TState>() where TState : IState
        {
            ChangeState(typeof(TState));
        }

        /// <summary>
        /// Register permitted states and designate the initial state; called by <c>_Ready</c> before
        /// <see cref="RegisterTransitions"/>.
        /// </summary>
        protected abstract void RegisterStates();

        /// <summary>
        /// Declare all valid transitions using the fluent <see cref="When{TSource}"/> builder;
        /// called by <c>_Ready</c> after <see cref="RegisterStates"/>.
        /// </summary>
        protected abstract void RegisterTransitions();

        /// <summary>Starts building a transition rule whose source state is <typeparamref name="TSource"/>.</summary>
        /// <typeparam name="TSource">The state that must be active for this rule to be eligible.</typeparam>
        protected TransitionBuilder When<TSource>() where TSource : IState
        {
            return new TransitionBuilder(typeof(TSource), _transitions);
        }

        /// <summary>Registers a new state instance keyed by its type; call from <see cref="RegisterStates"/>.</summary>
        /// <typeparam name="TState">State type; must have a public parameterless constructor.</typeparam>
        protected void RegisterState<TState>() where TState : IState, new()
        {
            _states[typeof(TState)] = new TState();
        }

        /// <summary>Designates the state the machine enters first; must be called from <see cref="RegisterStates"/>.</summary>
        /// <typeparam name="TState">Initial state type.</typeparam>
        protected void SetInitialState<TState>() where TState : IState
        {
            _initialStateType = typeof(TState);
        }

        /// <summary>Performs the Exit/Enter handoff. No-op if already in the target state.</summary>
        private void ChangeState(Type targetType)
        {
            bool stateExists = _states.TryGetValue(targetType, out IState? targetState);
            if (!stateExists)
            {
                throw new KeyNotFoundException(
                    $"{GetType().Name} does not have a registered state of type {targetType.Name}. " +
                    "Call RegisterState<T>() for this state in RegisterStates().");
            }

            bool isSameState = _currentState?.GetType() == targetType;
            if (!isSameState)
            {
                _currentState?.Exit(_actor!);
                EnterState(targetState!);
            }
        }

        /// <summary>
        /// Returns the target type of the first matching transition entry, or null if none matched.
        /// Walks the full list to honour single-return-point; evaluates predicates only when source
        /// matches and no prior match exists.
        /// </summary>
        private Type? FindTransitionTarget(ActionIntent intent)
        {
            Type currentType = _currentState!.GetType();
            Type? result = null;

            foreach (TransitionEntry entry in _transitions)
            {
                bool alreadyMatched = result != null;
                bool sourceMatches = entry.SourceType == currentType;

                if (!alreadyMatched && sourceMatches && entry.Predicate(intent))
                {
                    result = entry.TargetType;
                }
            }

            return result;
        }

        /// <summary>
        /// Validates that every source and target type in the transition table was registered.
        /// Throws at startup so misconfiguration surfaces immediately rather than at runtime.
        /// </summary>
        private void ValidateTransitionTable()
        {
            foreach (TransitionEntry entry in _transitions)
            {
                bool sourceRegistered = _states.ContainsKey(entry.SourceType);
                if (!sourceRegistered)
                {
                    throw new InvalidOperationException(
                        $"{GetType().Name}: transition table references source state " +
                        $"'{entry.SourceType.Name}' which was not registered via RegisterState<T>().");
                }

                bool targetRegistered = _states.ContainsKey(entry.TargetType);
                if (!targetRegistered)
                {
                    throw new InvalidOperationException(
                        $"{GetType().Name}: transition table references target state " +
                        $"'{entry.TargetType.Name}' which was not registered via RegisterState<T>().");
                }
            }
        }

        /// <summary>Assigns the state as current and calls its Enter; shared by <c>_Ready</c> and <see cref="ChangeState(Type)"/>.</summary>
        private void EnterState(IState state)
        {
            _currentState = state;
            _currentState.Enter(_actor!);
        }

        // -----------------------------------------------------------------------------------------
        // Fluent transition builder
        // -----------------------------------------------------------------------------------------

        /// <summary>
        /// First step of the fluent transition builder, scoped to a source state type.
        /// Accessible to subclasses via <see cref="When{TSource}"/>.
        /// </summary>
        protected sealed class TransitionBuilder
        {
            /// <summary>Source state type passed through to the next builder step.</summary>
            private readonly Type _sourceType;

            /// <summary>Reference to the machine's transition list.</summary>
            private readonly List<TransitionEntry> _transitions;

            /// <summary>Initialises a builder scoped to the given source type.</summary>
            internal TransitionBuilder(Type sourceType, List<TransitionEntry> transitions)
            {
                _sourceType = sourceType;
                _transitions = transitions;
            }

            /// <summary>Advances the builder by supplying a typed predicate over <typeparamref name="TIntent"/>.</summary>
            /// <typeparam name="TIntent">The intent subtype this transition reacts to.</typeparam>
            /// <param name="predicate">Returns true when the intent satisfies the transition condition.</param>
            public TransitionPredicateBuilder<TIntent> On<TIntent>(Func<TIntent, bool> predicate)
                where TIntent : ActionIntent
            {
                return new TransitionPredicateBuilder<TIntent>(_sourceType, predicate, _transitions);
            }
        }

        /// <summary>
        /// Second step of the fluent transition builder; holds source type and predicate,
        /// awaiting the target type via <see cref="Transition{TTarget}"/>.
        /// </summary>
        /// <typeparam name="TIntent">The intent subtype the predicate was declared for.</typeparam>
        protected sealed class TransitionPredicateBuilder<TIntent> where TIntent : ActionIntent
        {
            /// <summary>Source state type from the preceding <see cref="TransitionBuilder"/>.</summary>
            private readonly Type _sourceType;

            /// <summary>Typed predicate; wrapped into a raw delegate in <see cref="Transition{TTarget}"/>.</summary>
            private readonly Func<TIntent, bool> _typedPredicate;

            /// <summary>Reference to the machine's transition list.</summary>
            private readonly List<TransitionEntry> _transitions;

            /// <summary>Initialises with source type, typed predicate, and target list.</summary>
            internal TransitionPredicateBuilder(
                Type sourceType,
                Func<TIntent, bool> typedPredicate,
                List<TransitionEntry> transitions)
            {
                _sourceType = sourceType;
                _typedPredicate = typedPredicate;
                _transitions = transitions;
            }

            /// <summary>
            /// Completes the rule by appending the entry to the transition table. The typed predicate
            /// is wrapped into a raw delegate that type-tests before delegating, allowing heterogeneous
            /// predicates to coexist in a single list.
            /// </summary>
            /// <typeparam name="TTarget">Target state; must be registered via <see cref="RegisterState{TState}"/>.</typeparam>
            public void Transition<TTarget>() where TTarget : IState
            {
                Func<ActionIntent, bool> wrappedPredicate =
                    raw => raw is TIntent typed && _typedPredicate(typed);

                _transitions.Add(new TransitionEntry(_sourceType, wrappedPredicate, typeof(TTarget)));
            }
        }
    }
}
