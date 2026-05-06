using System;
using Vikare.Entities.Abilities;

namespace Vikare.Entities.States
{
    /// <summary> The actor wants to do something special. The logic of which is handled by the triggering <see cref="AbilityEffect"/>. </summary>
    /// <remarks> This state is just a wrapper for AbilityEffects. In this configuration, this state works like a state machine, with the effect working like a state (just one shared with **every** actor). </remarks>
    public sealed class AbilityState : IState
    {
        /// <inheritdoc/>
        public String AnimationName => "";


        /// <summary> The currently active ability. </summary>
        private AbilityEffect? _currentAbility = null;


        /// <inheritdoc/>
        /// <remarks> Despite it's name, this isn't used here. The initialise is actually HandleIntent. </remarks>
        public void Enter(Actor actor) { }


        /// <inheritdoc/>
        public void Exit(Actor actor)
        {
            _currentAbility = null;
        }


        /// <inheritdoc/>
        public void Process(Actor actor, Double delta) => _currentAbility?.Process(delta, actor);


        /// <inheritdoc/>
        public void PhysicsProcess(Actor actor, Double delta) => _currentAbility?.PhysicsProcess(delta, actor);


        /// <inheritdoc/>
        public void HandleIntent(Actor actor, ActionIntent intent)
        {
            if (intent is AbilityIntent abilityIntent)
            {
                _currentAbility = abilityIntent.Ability;
                actor.PlayAnimation(_currentAbility.AnimationName, actor.Direction);
            }
        }


        public void Transition<T>(Actor actor) where T : IState => actor.Machine.ChangeState<T>();
    }
}
