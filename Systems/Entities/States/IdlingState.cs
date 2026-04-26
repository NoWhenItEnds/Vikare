using Vikare.Entities.Intents;
using Vikare.Entities.Interfaces;

namespace Vikare.Entities.States
{
    /// <summary>
    /// Resting state — zeroes velocity and plays the idle animation. Transitions are driven entirely
    /// by the machine's transition table; this state holds no per-intent data.
    /// </summary>
    public sealed class IdlingState : IState
    {
        /// <summary>Idle animation clip name; must match the entity's animation library.</summary>
        private const string IdleAnimationName = "idle";

        /// <summary>Zeroes velocity and plays the idle animation.</summary>
        public void Enter(Actor actor)
        {
            actor.ZeroVelocity();
            actor.PlayAnimation(IdleAnimationName);
        }

        /// <inheritdoc/>
        public void Exit(Actor actor)
        {
        }

        /// <inheritdoc/>
        public void Process(Actor actor, double delta)
        {
        }

        /// <inheritdoc/>
        public void PhysicsProcess(Actor actor, double delta)
        {
        }

        /// <inheritdoc/>
        public void HandleIntent(Actor actor, ActionIntent intent)
        {
        }
    }
}
