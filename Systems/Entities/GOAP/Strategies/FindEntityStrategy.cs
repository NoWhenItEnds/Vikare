using Godot;
using System;

namespace Vikare.Entities.GOAP.Strategies
{
    /// <summary> An actor attempts to find the target. </summary>
    public class FindEntityStrategy : IActionStrategy
    {
        /// <inheritdoc/>
        public Boolean IsValid => throw new NotImplementedException();

        /// <inheritdoc/>
        public Boolean IsComplete => throw new NotImplementedException();


        /// <summary> A reference to the actor being manipulated. </summary>
        private readonly Actor _actor;

        /// <summary> The strategy's target entity. </summary>
        private readonly Entity _targetEntity;


        /// <summary> An actor attempts to find the target. </summary>
        /// <param name="actor"> A reference to the actor being manipulated. </param>
        /// <param name="targetEntity"> The strategy's target entity. </param>
        public FindEntityStrategy(Actor actor, Entity targetEntity)
        {
            _actor = actor;
            _targetEntity = targetEntity;
        }


        /// <inheritdoc/>
        public void Start()
        {
            throw new NotImplementedException();
        }


        /// <inheritdoc/>
        public void Update(double delta)
        {
            throw new NotImplementedException();
        }


        /// <inheritdoc/>
        public void Stop()
        {
            throw new NotImplementedException();
        }
    }
}
