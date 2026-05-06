using System;
using System.Collections.Generic;
using Vikare.Entities.GOAP.Strategies;

namespace Vikare.Entities.GOAP
{
    /// <summary> A potential action an entity can use to try to address a goal. </summary>
    public class ActorAction : IEquatable<ActorAction>
    {
        /// <summary> The name or key identifying the action. </summary>
        public String Name { get; }

        /// <summary> The function to use for calculating the action's current cost (higher is obviously more costly). </summary>
        public Func<Single> Cost { get; private set; } = () => 1f;

        /// <summary> The facts or conditions that need to be true for the action to be actioned. </summary>
        public HashSet<ActorFact> Preconditions { get; } = new HashSet<ActorFact>();

        /// <summary> How the actor's facts or state will change as a result of the action. </summary>
        public HashSet<ActorFact> Outcomes { get; } = new HashSet<ActorFact>();

        /// <summary> Whether the action is now complete. </summary>
        public Boolean IsComplete => _strategy.IsComplete;


        /// <summary> A reference to the strategy / logic used for this action. </summary>
        private readonly IActionStrategy _strategy;


        /// <summary> A potential action an entity can use to try to address a goal. </summary>
        /// <param name="name"> The name or key identifying the action. </param>
        /// <param name="strategy"> A reference to the strategy / logic used for this action. </param>
        private ActorAction(String name, IActionStrategy strategy)
        {
            Name = name;
            _strategy = strategy;
        }


        /// <summary> Begin carrying out the action. </summary>
        public void Start() => _strategy.Start();


        /// <summary> Update / incrementally run the current action. </summary>
        /// <param name="delta"> The time since the last update frame. </param>
        public void Update(Double delta)
        {
            if (_strategy.IsValid)
            {
                _strategy.Update(delta);
            }

            // Bail out if the strategy is still executing
            if (!_strategy.IsComplete) { return; }

            // Check to see if any outcomes have been met.
            foreach (ActorFact outcome in Outcomes)
            {
                outcome.Evaluate();
            }
        }


        /// <summary> Stop or cancel the currently running action. Ensures this is done gracefully. </summary>
        public void Stop() => _strategy.Stop();


        /// <inheritdoc/>
        public override Int32 GetHashCode() => HashCode.Combine(Name);


        /// <inheritdoc/>
        public override Boolean Equals(Object? obj)
        {
            ActorAction? other = obj as ActorAction;
            return other != null ? Name.Equals(other.Name) : false;
        }


        /// <inheritdoc/>
        public Boolean Equals(ActorAction? other) => other != null ? Name.Equals(other.Name) : false;


        /// <summary> A helpful builder that allows for easy construction of actor actions. </summary>
        public class Builder
        {
            /// <summary> A reference to the action being constructed. </summary>
            private readonly ActorAction _action;


            /// <summary> A helpful builder that allows for easy construction of actor actions. </summary>
            /// <param name="name"> The name or key identifying the action. </param>
            /// <param name="strategy"> A reference to the strategy / logic used for this action. </param>
            public Builder(String name, IActionStrategy strategy)
            {
                _action = new ActorAction(name, strategy);
            }


            /// <summary> Sets the action cost using a function calculated at runtime. </summary>
            /// <param name="cost"> The cost function to calculate how many 'action points' the action would cost to action. </param>
            public Builder WithCost(Func<Single> cost)
            {
                _action.Cost = cost;
                return this;
            }


            /// <summary> Sets the action cost as a static value. </summary>
            /// <param name="cost"> How many 'action points' the action would cost to action. </param>
            public Builder WithCost(Single cost)
            {
                _action.Cost = () => cost;
                return this;
            }


            /// <summary> Sets the action cost as a result of the distance between the actor and another entity. </summary>
            /// <param name="actor"> The actor performing the action. </param>
            /// <param name="other"> The entity to check. </param>
            public Builder WithDistanceCost(Actor actor, Entity other)
            {
                _action.Cost = () => actor.GlobalPosition.DistanceTo(other.GlobalPosition);
                return this;
            }


            /// <summary> Adds a precondition to the action that must be true for the action to begin. </summary>
            /// <param name="precondition"> The facts or conditions that need to be true for the action to be actioned. </param>
            public Builder AddPrecondition(ActorFact precondition)
            {
                _action.Preconditions.Add(precondition);
                return this;
            }


            /// <summary> Adds preconditions to the action that must be true for the action to begin. </summary>
            /// <param name="preconditions"> The facts or conditions that need to be true for the action to be actioned. </param>
            public Builder AddPrecondition(ActorFact[] preconditions)
            {
                foreach (ActorFact precondition in preconditions)
                {
                    _action.Preconditions.Add(precondition);
                }
                return this;
            }


            /// <summary> Adds a outcome that is fulfilled by the action being completed. </summary>
            /// <param name="outcome"> How the actor's facts or state will change as a result of the action. </param>
            public Builder AddOutcome(ActorFact outcome)
            {
                _action.Outcomes.Add(outcome);
                return this;
            }


            /// <summary> Adds outcomes that is fulfilled by the action being completed. </summary>
            /// <param name="outcomes"> How the actor's facts or state will change as a result of the action. </param>
            public Builder AddOutcome(ActorFact[] outcomes)
            {
                foreach (ActorFact outcome in outcomes)
                {
                    _action.Outcomes.Add(outcome);
                }
                return this;
            }


            /// <summary> Build the architected action. </summary>
            /// <returns> The newly constructed action. </returns>
            public ActorAction Build()
            {
                return _action;
            }
        }
    }
}
