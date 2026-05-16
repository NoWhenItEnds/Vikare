using System;
using Godot;
using Vikare.Entities.States;

namespace Vikare.Entities.GOAP.Strategies
{
    /// <summary> An actor uses one of the advertisers hosted by an entity. </summary>
    /// <remarks> Validity is revoked during <see cref="Update"/> if the host is freed or the actor moves out of range; once invalid, the state stays invalid so the planner can re-plan. </remarks>
    public class UseAdvertiserStrategy : IActionStrategy
    {
        /// <inheritdoc/>
        public Boolean IsValid { get; private set; } = true;

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


        /// <summary>
        /// Resets elapsed time, clears the completion flag, clears the abort-notified flag, and
        /// transitions the actor's FSM into <c>UsingState</c> via <see cref="UseIntent"/>.
        /// Does not restore validity — once invalid, the strategy stays invalid so the planner re-plans.
        /// </summary>
        public void Start()
        {
            _elapsed = 0;
            IsComplete = false;
            _abortNotified = false;
            _actor.Machine.HandleIntent(new UseIntent());
        }


        /// <summary>
        /// Validates proximity and host lifetime, advances the elapsed timer, and fires
        /// <see cref="_onTick"/> each frame; fires <see cref="_onComplete"/> exactly once when
        /// the duration is reached. Calls <c>ChangeState&lt;IdlingState&gt;()</c> (at most once,
        /// guarded by <see cref="_abortNotified"/>) when proximity or host validity fails.
        /// </summary>
        /// <param name="delta"> Time in seconds since the last update. </param>
        public void Update(Double delta)
        {
            Boolean hostAlive = GodotObject.IsInstanceValid(_host);
            Boolean inRange = false;

            if (hostAlive)
            {
                Single distanceSquared = _actor.GlobalPosition.DistanceSquaredTo(_host.GlobalPosition);
                inRange = distanceSquared <= _interactionRadiusSquared;
            }

            if (hostAlive && inRange)
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
                IsValid = false;

                if (!_abortNotified)
                {
                    _actor.Machine.ChangeState<IdlingState>();
                    _abortNotified = true;
                }
            }
        }


        /// <summary>
        /// Transitions the actor's FSM out of <c>UsingState</c> and back to <c>IdlingState</c>
        /// directly via <c>ChangeState</c>. Called by <c>ActorController</c> when <see cref="IsComplete"/> is true.
        /// </summary>
        public void Stop()
        {
            _actor.Machine.ChangeState<IdlingState>();
        }
    }
}
