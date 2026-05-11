using System;
using Godot;
using Vikare.Entities.GOAP.Advertisers;

namespace Vikare.Entities.GOAP.Strategies
{
    /// <summary> An actor uses the given advertiser. </summary>
    public class UseAdvertiserStrategy<T> : IActionStrategy where T : ActionAdvertiser
    {
        /// <inheritdoc/>
        public Boolean IsValid => true; // TODO - Not sure what to use here.

        /// <inheritdoc/>
        public Boolean IsComplete => _actor.NavigationAgent.IsNavigationFinished();

        /// <summary> A reference to the actor being manipulated. </summary>
        private readonly Actor _actor;

        /// <summary> The strategy's target entity. </summary>
        private readonly Entity _targetEntity;


        /// <summary> An actor moves itself to the given entity's position. </summary>
        /// <param name="actor"> A reference to the actor being manipulated. </param>
        /// <param name="targetEntity"> The strategy's target entity. </param>
        public UseAdvertiserStrategy(Actor actor)
        {
            _actor = actor;
            _targetEntity = targetEntity;
        }


        /// <inheritdoc/>
        public void Start()
        {
            _actor.NavigationAgent.TargetPosition = _targetEntity.GlobalPosition;
        }


        /// <inheritdoc/>
        public void Update(Double delta)
        {
            Vector2 nextPosition = _actor.NavigationAgent.GetNextPathPosition();
            WalkIntent intent = new WalkIntent(nextPosition.Normalized());
            _actor.Machine.HandleIntent(intent);
        }


        /// <inheritdoc/>
        public void Stop()
        {
            _actor.NavigationAgent.TargetPosition = _actor.GlobalPosition;
        }
    }
}
