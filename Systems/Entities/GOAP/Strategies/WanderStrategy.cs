using Godot;
using Godot.Collections;
using System;

namespace Vikare.Entities.GOAP.Strategies
{
    /// <summary> The actor wanders aimlessly. </summary>
    public class WanderStrategy : IActionStrategy
    {
        /// <inheritdoc/>
        public Boolean IsValid => true; // TODO - Not sure what to use here.

        /// <inheritdoc/>
        public Boolean IsComplete => _actor.NavigationAgent.IsNavigationFinished();

        /// <summary> A reference to the actor being manipulated. </summary>
        private readonly Actor _actor;


        /// <summary> The actor wanders aimlessly. </summary>
        /// <param name="actor"> A reference to the actor being manipulated. </param>
        public WanderStrategy(Actor actor)
        {
            _actor = actor;
        }


        /// <inheritdoc/>
        public void Start()
        {
            Rid navigationMap = _actor.GetWorld2D().NavigationMap;
            Vector2 location = NavigationServer2D.MapGetRandomPoint(navigationMap, 1, false);
            _actor.NavigationAgent.TargetPosition = location;
        }


        /// <inheritdoc/>
        public void Update(Double delta)
        {
            Vector2 nextPosition = _actor.NavigationAgent.GetNextPathPosition();
            Vector2 direction = _actor.GlobalPosition.DirectionTo(nextPosition);
            WalkIntent intent = new WalkIntent(direction);
            _actor.Machine.HandleIntent(intent);
        }


        /// <inheritdoc/>
        public void Stop()
        {
            _actor.NavigationAgent.TargetPosition = _actor.GlobalPosition;
        }
    }
}
