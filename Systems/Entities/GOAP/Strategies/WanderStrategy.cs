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
            // For some reason, we can't use the map directly, so we use it to get the regions instead, which we can use.
            // TODO - Might be able to manually set the navigation map with a region manager (https://docs.godotengine.org/en/latest/tutorials/navigation/navigation_using_navigationmaps.html).
            Rid navigationMap = _actor.GetWorld2D().NavigationMap;
            Array<Rid> regions = NavigationServer3D.MapGetRegions(navigationMap);
            if (regions.Count > 0)
            {
                Vector2 location = NavigationServer2D.RegionGetRandomPoint(regions[0], 0, true);    // TODO - Else will need random here.
                _actor.NavigationAgent.TargetPosition = location;
            }
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
