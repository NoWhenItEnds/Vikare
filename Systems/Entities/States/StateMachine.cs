using System;
using System.Collections.Generic;
using Godot;

namespace Vikare.Entities.States
{
    /// <summary>
    /// Node-based finite state machine base class. Subclass it, override <see cref="RegisterStates"/> to call
    /// <see cref="RegisterState{TState}"/> and <see cref="SetInitialState{TState}"/>, then override
    /// <see cref="RegisterTransitions"/> to declare transitions via the fluent <see cref="When{TSource}"/> builder.
    /// Attach as a child of the entity node.
    /// </summary>
    public abstract partial class StateMachine : Node
    {
        /// <summary>
        /// A single row in the transition table: the source state type, a predicate over a raw intent, and the target state type.
        /// </summary>
        internal readonly struct TransitionEntry
        {
            /// <summary>
            /// Concrete type of the state that must be active for this transition to be eligible.
            /// </summary>
            public Type SourceType { get; }

            /// <summary>
            /// Returns true when the supplied intent satisfies the condition for this transition.
            /// </summary>
            public Func<ActionIntent, bool> Predicate { get; }

            /// <summary>
            /// Concrete type of the state to enter when the predicate matches.
            /// </summary>
            public Type TargetType { get; }

            /// <summary>
            /// Initialises a new <see cref="TransitionEntry"/> with all three fields.
            /// </summary>
            /// <param name="sourceType">Source state type.</param>
            /// <param name="predicate">Intent predicate; must return true for the transition to fire.</param>
            /// <param name="targetType">Target state type.</param>
            public TransitionEntry(Type sourceType, Func<ActionIntent, Boolean> predicate, Type targetType)
            {
                SourceType = sourceType;
                Predicate = predicate;
                TargetType = targetType;
            }
        }

        /// <summary>
        /// Ordered list of all registered transitions; consulted in registration order on every <see cref="HandleIntent"/> call.
        /// </summary>
        private readonly List<TransitionEntry> _transitions = new();

        /// <summary>
        /// Type of the state the machine enters first; set by <see cref="SetInitialState{TState}"/> inside <see cref="RegisterStates"/>.
        /// </summary>
        private Type? _initialStateType;

        /// <summary>
        /// All states registered for this machine, keyed by concrete <see cref="Type"/>; populated during <see cref="RegisterStates"/>.
        /// </summary>
        private readonly Dictionary<Type, IState> _states = new();

        /// <summary>
        /// The currently active state; null only between construction and <c>_Ready</c> completing.
        /// </summary>
        private IState? _currentState;

        /// <summary>
        /// Context passed to every state call; created once in <c>_Ready</c> from the parent node and reused for the machine's lifetime.
        /// </summary>
        private IStateContext? _context;

        /// <summary>
        /// Resolves the parent entity context, registers states, registers transitions, validates the transition table,
        /// then enters the initial state. Throws <see cref="InvalidOperationException"/> if either registration step is incomplete.
        /// </summary>
        public override void _Ready()
        {
            Node parent = GetParent();
            _context = new StateContext(parent);

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

        /// <summary>
        /// Forwards the visual-frame tick to the active state.
        /// </summary>
        /// <param name="delta">Elapsed time since the last frame, in seconds.</param>
        public override void _Process(double delta)
        {
            _currentState?.Process(_context!, delta);
        }

        /// <summary>
        /// Forwards the physics tick to the active state.
        /// </summary>
        /// <remarks>
        /// Godot executes <c>_PhysicsProcess</c> parent-before-child, so <c>Entity._PhysicsProcess</c> calls
        /// <c>MoveAndSlide</c> before this runs. Velocity written by the active state at tick N is consumed at tick N+1.
        /// See <c>Entity._PhysicsProcess</c> for the full tick-ordering contract.
        /// </remarks>
        /// <param name="delta">Elapsed time since the last physics tick, in seconds.</param>
        public override void _PhysicsProcess(double delta)
        {
            _currentState?.PhysicsProcess(_context!, delta);
        }

        /// <summary>
        /// Consults the transition table for the current state, fires the first matching transition (if any),
        /// then delivers the intent to the (now-current) state so it can update its internal data.
        /// Silently dropped if the machine has not yet initialised.
        /// </summary>
        /// <param name="intent">The controller's intent. Must not be null.</param>
        public void HandleIntent(ActionIntent intent)
        {
            bool isReady = _currentState != null && _context != null;
            if (isReady)
            {
                Type? matchedTarget = FindTransitionTarget(intent);
                bool transitionFound = matchedTarget != null;
                if (transitionFound)
                {
                    ChangeState(matchedTarget!);
                }

                _currentState!.HandleIntent(_context!, intent);
            }
        }

        /// <summary>
        /// Transitions to <typeparamref name="TState"/> by calling <see cref="IState.Exit"/> then <see cref="IState.Enter"/>.
        /// No-op if already in <typeparamref name="TState"/>. Throws <see cref="KeyNotFoundException"/> if the type was not registered.
        /// </summary>
        /// <remarks>
        /// Exit and Enter both complete before this call returns; prefer to make <c>ChangeState</c> the last statement in the calling branch.
        /// Re-entrant transitions are not supported — do not call <c>ChangeState</c> from within <c>Enter</c> or <c>Exit</c>.
        /// </remarks>
        /// <typeparam name="TState">Concrete state type to transition to; must have been registered via <see cref="RegisterState{TState}"/>.</typeparam>
        public void ChangeState<TState>() where TState : IState
        {
            ChangeState(typeof(TState));
        }

        /// <summary>
        /// Implemented by subclasses to register permitted states and designate the initial state via
        /// <see cref="RegisterState{TState}"/> and <see cref="SetInitialState{TState}"/>.
        /// </summary>
        protected abstract void RegisterStates();

        /// <summary>
        /// Implemented by subclasses to declare all valid transitions using the fluent <see cref="When{TSource}"/> builder.
        /// Called by <c>_Ready</c> after <see cref="RegisterStates"/> completes.
        /// </summary>
        protected abstract void RegisterTransitions();

        /// <summary>
        /// Starts building a transition rule whose source state is <typeparamref name="TSource"/>.
        /// </summary>
        /// <typeparam name="TSource">The state that must be active for this rule to be eligible.</typeparam>
        /// <returns>A builder scoped to <typeparamref name="TSource"/>.</returns>
        protected TransitionBuilder When<TSource>() where TSource : IState
        {
            return new TransitionBuilder(typeof(TSource), _transitions);
        }

        /// <summary>
        /// Registers a new state instance keyed by its concrete type; call exclusively from <see cref="RegisterStates"/>.
        /// </summary>
        /// <typeparam name="TState">State type to register; must have a public parameterless constructor.</typeparam>
        protected void RegisterState<TState>() where TState : IState, new()
        {
            Type stateType = typeof(TState);
            _states[stateType] = new TState();
        }

        /// <summary>
        /// Designates which registered state the machine enters first after <c>_Ready</c>; must be called from <see cref="RegisterStates"/>.
        /// </summary>
        /// <typeparam name="TState">Initial state type; must already be registered.</typeparam>
        protected void SetInitialState<TState>() where TState : IState
        {
            _initialStateType = typeof(TState);
        }

        /// <summary>
        /// Non-generic transition helper shared by the public generic overload and the table-driven <see cref="HandleIntent"/> path.
        /// Performs the Exit/Enter handoff. No-op if already in the target state.
        /// Throws <see cref="KeyNotFoundException"/> if the type was not registered.
        /// </summary>
        /// <param name="targetType">Concrete type of the state to transition to.</param>
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
                _currentState?.Exit(_context!);
                EnterState(targetState!);
            }
        }

        /// <summary>
        /// Walks the transition table in registration order and returns the target type of the first entry whose
        /// source matches the current state and whose predicate matches the intent. Returns null if no entry matches.
        /// The full list is always walked (no early exit) to honour the single-return-point constraint;
        /// the predicate is only evaluated when the source type matches and no prior match exists.
        /// </summary>
        /// <param name="intent">The intent to test against each predicate.</param>
        /// <returns>The target state type of the first matching transition, or null.</returns>
        private Type? FindTransitionTarget(IInputIntent intent)
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
        /// Validates that every source and target type in the transition table was registered via <see cref="RegisterState{TState}"/>.
        /// Throws <see cref="InvalidOperationException"/> at startup if any type is missing.
        /// </summary>
        private void ValidateTransitionTable()
        {
            foreach (TransitionEntry entry in _transitions)
            {
                bool sourceRegistered = _states.ContainsKey(entry.SourceType);
                if (!sourceRegistered)
                {
                    throw new InvalidOperationException(
                        $"{GetType().Name}: transition table references source state '{entry.SourceType.Name}' " +
                        "which was not registered via RegisterState<T>().");
                }

                bool targetRegistered = _states.ContainsKey(entry.TargetType);
                if (!targetRegistered)
                {
                    throw new InvalidOperationException(
                        $"{GetType().Name}: transition table references target state '{entry.TargetType.Name}' " +
                        "which was not registered via RegisterState<T>().");
                }
            }
        }

        /// <summary>
        /// Assigns the incoming state as current and calls its <see cref="IState.Enter"/>; shared entry path for <c>_Ready</c> and <see cref="ChangeState(Type)"/>.
        /// </summary>
        /// <param name="state">The state to enter. Must not be null.</param>
        private void EnterState(IState state)
        {
            _currentState = state;
            _currentState.Enter(_context!);
        }

        // -----------------------------------------------------------------------------------------
        // Fluent transition builder — infrastructure types, not states.
        // -----------------------------------------------------------------------------------------

        /// <summary>
        /// First step of the fluent transition builder; scoped to a specific source state type.
        /// Only accessible to subclasses via the protected <see cref="When{TSource}"/> factory method.
        /// </summary>
        protected sealed class TransitionBuilder
        {
            /// <summary>
            /// The source state type this builder is scoped to.
            /// </summary>
            private readonly Type _sourceType;

            /// <summary>
            /// Reference to the machine's transition list; entries are added here by <see cref="TransitionPredicateBuilder{TIntent}.Transition{TTarget}"/>.
            /// </summary>
            private readonly List<TransitionEntry> _transitions;

            /// <summary>
            /// Initialises a <see cref="TransitionBuilder"/> for the given source type, writing into the supplied list.
            /// </summary>
            /// <param name="sourceType">The state type that must be active for transitions registered through this builder to be eligible.</param>
            /// <param name="transitions">The machine's transition list to append to.</param>
            internal TransitionBuilder(Type sourceType, List<TransitionEntry> transitions)
            {
                _sourceType = sourceType;
                _transitions = transitions;
            }

            /// <summary>
            /// Advances the builder by supplying a typed predicate over <typeparamref name="TIntent"/>.
            /// </summary>
            /// <typeparam name="TIntent">The specific <see cref="IInputIntent"/> subtype this transition reacts to.</typeparam>
            /// <param name="predicate">Returns true when the intent satisfies the transition condition.</param>
            /// <returns>A builder ready to accept the target state via <see cref="TransitionPredicateBuilder{TIntent}.Transition{TTarget}"/>.</returns>
            public TransitionPredicateBuilder<TIntent> On<TIntent>(Func<TIntent, bool> predicate)
                where TIntent : IInputIntent
            {
                return new TransitionPredicateBuilder<TIntent>(_sourceType, predicate, _transitions);
            }
        }

        /// <summary>
        /// Second step of the fluent transition builder; holds source type and predicate, waiting for the target type.
        /// </summary>
        /// <typeparam name="TIntent">The specific intent subtype the predicate was declared for.</typeparam>
        protected sealed class TransitionPredicateBuilder<TIntent> where TIntent : IInputIntent
        {
            /// <summary>
            /// The source state type captured from the preceding <see cref="TransitionBuilder"/>.
            /// </summary>
            private readonly Type _sourceType;

            /// <summary>
            /// The typed predicate captured from <see cref="TransitionBuilder.On{TIntent}"/>; wrapped to accept the raw <see cref="IInputIntent"/> interface.
            /// </summary>
            private readonly Func<TIntent, bool> _typedPredicate;

            /// <summary>
            /// Reference to the machine's transition list; the entry is appended here by <see cref="Transition{TTarget}"/>.
            /// </summary>
            private readonly List<TransitionEntry> _transitions;

            /// <summary>
            /// Initialises the second builder step with the source type, typed predicate, and the target list.
            /// </summary>
            /// <param name="sourceType">Source state type.</param>
            /// <param name="typedPredicate">Typed predicate that was provided by the caller.</param>
            /// <param name="transitions">The machine's transition list to append to.</param>
            internal TransitionPredicateBuilder(Type sourceType, Func<TIntent, bool> typedPredicate, List<TransitionEntry> transitions)
            {
                _sourceType = sourceType;
                _typedPredicate = typedPredicate;
                _transitions = transitions;
            }

            /// <summary>
            /// Completes the rule by specifying the target state and appending the entry to the machine's transition table.
            /// </summary>
            /// <typeparam name="TTarget">The state to enter when the predicate matches.</typeparam>
            public void Transition<TTarget>() where TTarget : IState
            {
                Func<IInputIntent, bool> wrappedPredicate =
                    raw => raw is TIntent typed && _typedPredicate(typed);

                _transitions.Add(new TransitionEntry(_sourceType, wrappedPredicate, typeof(TTarget)));
            }
        }
    }
}
