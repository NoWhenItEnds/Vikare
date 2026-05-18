using System;
using Vikare.Entities.States;

namespace Vikare.Entities.GOAP.Strategies
{
    /// <summary> An actor uses one of the advertisers hosted by an entity. </summary>
    /// <remarks> Validity is revoked during <see cref="Update"/> if the host is freed or the actor moves out of range; once invalid, the state stays invalid so the planner can re-plan. </remarks>
    public class UseAdvertiserStrategy : IActionStrategy
    {
        /// <inheritdoc/>
        public Boolean IsValid => _interactionRadiusSquared < _actor.GlobalPosition.DistanceSquaredTo(_host.GlobalPosition);

        /// <inheritdoc/>
        public Boolean IsComplete { get; private set; } = false;


        /// <summary> The actor performing the interaction. </summary>
        private readonly Actor _actor;

        /// <summary> The entity being interacted with. </summary>
        private readonly Entity _host;

        /// <summary> Total time in seconds the interaction takes to complete. </summary>
        private readonly Single _duration;

        /// <summary>
        /// Squared maximum distance (in world units) the actor may be from the host.
        /// Stored squared to avoid a square-root call on every <see cref="Update"/>.
        /// </summary>
        private readonly Single _interactionRadiusSquared;

        /// <summary> Invoked on every <see cref="Update"/> tick whilst the interaction is valid and incomplete. Receives the frame delta already narrowed to <see cref="Single"/>. </summary>
        private readonly Action<Single> _onTick;

        /// <summary> Invoked exactly once when the elapsed time reaches <see cref="_duration"/>. </summary>
        private readonly Action _onComplete;

        /// <summary> Accumulated time in seconds since <see cref="Start"/> was last called. </summary>
        private Double _elapsed;

        /// <summary>
        /// Guards against calling <c>ChangeState&lt;IdlingState&gt;()</c> more than once when the
        /// strategy becomes invalid mid-run; set to true the first time the state change is issued
        /// in <see cref="Update"/>. Required because <c>ActorController</c> does not clear
        /// <c>CurrentAction</c> on <see cref="IsValid"/> flipping false, so <see cref="Update"/>
        /// keeps running every tick after an abort.
        /// </summary>
        private Boolean _abortNotified;


        /// <summary> An actor stands within range of a host entity for a fixed duration. </summary>
        /// <param name="actor"> The actor performing the interaction. </param>
        /// <param name="host"> The entity being interacted with. </param>
        /// <param name="duration"> Total time in seconds the interaction takes to complete. </param>
        /// <param name="interactionRadius"> Maximum distance in world units the actor may be from the host; stored internally as the squared value. </param>
        /// <param name="onTick"> Invoked each <see cref="Update"/> with the frame delta (narrowed to <see cref="Single"/>) whilst the interaction is active. </param>
        /// <param name="onComplete"> Invoked once when the full duration elapses. </param>
        public UseAdvertiserStrategy(
            Actor actor,
            Entity host,
            Single duration,
            Single interactionRadius,
            Action<Single> onTick,
            Action onComplete)
        {
            _actor = actor;
            _host = host;
            _duration = duration;
            _interactionRadiusSquared = interactionRadius * interactionRadius;
            _onTick = onTick;
            _onComplete = onComplete;
        }


        /// <inheritdoc/>
        public void Start()
        {
            _elapsed = 0;
            IsComplete = false;
            _abortNotified = false;
            _actor.Machine.HandleIntent(new UseIntent());
        }


        /// <inheritdoc/>
        public void Update(Double delta)
        {
            Single distanceSquared = _actor.GlobalPosition.DistanceSquaredTo(_host.GlobalPosition);
            Boolean inRange = distanceSquared <= _interactionRadiusSquared;

            if (inRange)
            {
                _onTick((Single)delta);
                _elapsed += delta;

                if (_elapsed >= _duration && !IsComplete)
                {
                    _onComplete();
                    IsComplete = true;
                }
            }
            else
            {
                if (!_abortNotified)
                {
                    _actor.Machine.ChangeState<IdlingState>();
                    _abortNotified = true;
                }
            }
        }


        /// <inheritdoc/>
        public void Stop()
        {
            _actor.Machine.ChangeState<IdlingState>();
            IsComplete = false;
        }
    }
}
