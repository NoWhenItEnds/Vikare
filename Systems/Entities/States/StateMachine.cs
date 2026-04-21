using System;
using System.Collections.Generic;
using Godot;

namespace Vikare.Entities.States
{
    /// <summary>
    /// Node-based finite state machine base class. Subclass it, override <see cref="RegisterStates"/> to call
    /// <see cref="RegisterState{TState}"/> and <see cref="SetInitialState{TState}"/>, then attach as a child of the entity.
    /// </summary>
    public abstract partial class StateMachine : Node
    {
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
        /// Resolves the parent entity context, registers states, then enters the initial state.
        /// Throws <see cref="InvalidOperationException"/> if <see cref="RegisterStates"/> did not call <see cref="SetInitialState{TState}"/>.
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
        /// Delivers a controller intent to the active state; silently dropped if the machine has not yet initialised.
        /// </summary>
        /// <param name="intent">The controller's intent. Must not be null.</param>
        public void HandleIntent(Controllers.IInputIntent intent)
        {
            bool isReady = _currentState != null && _context != null;
            if (isReady)
            {
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
            Type targetType = typeof(TState);

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
        /// Implemented by subclasses to register permitted states and designate the initial state via
        /// <see cref="RegisterState{TState}"/> and <see cref="SetInitialState{TState}"/>.
        /// </summary>
        protected abstract void RegisterStates();

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
        /// Assigns the incoming state as current and calls its <see cref="IState.Enter"/>; shared entry path for <c>_Ready</c> and <see cref="ChangeState{TState}"/>.
        /// </summary>
        /// <param name="state">The state to enter. Must not be null.</param>
        private void EnterState(IState state)
        {
            _currentState = state;
            _currentState.Enter(_context!);
        }
    }
}
