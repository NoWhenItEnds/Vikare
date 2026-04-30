using System;
using Vikare.Entities.Abilities;

namespace Vikare.Entities.States
{
    /// <summary> The actor wants to do something special. The logic of which is handled by the triggering <see cref="AbilityEffect"/>. </summary>
    /// <remarks> This state is just a wrapper for AbilityEffects, allowing the resources to define the ability-specific logic. </remarks>
    public sealed class AbilityState : IState
    {
        /// <summary> The currently active ability. </summary>
        private AbilityEffect? _currentAbility = null;

        /// <inheritdoc/>
        public void Enter(Actor actor)
        {
        }

        /// <inheritdoc/>
        public void Exit(Actor actor)
        {
            _currentAbility?.Exit(actor);
            _currentAbility = null;
        }

        /// <inheritdoc/>
        public void Process(Actor actor, Double delta)
        {
            _currentAbility?.Process(actor, delta);
        }


        /// <inheritdoc/>
        public void PhysicsProcess(Actor actor, Double delta)
        {
            _currentAbility?.PhysicsProcess(actor, delta);
        }


        /// <inheritdoc/>
        public void HandleIntent(Actor actor, ActionIntent intent)
        {
            if (intent is AbilityIntent abilityIntent)
            {
                _currentAbility = abilityIntent.Ability;
            }
        }
    }
}
