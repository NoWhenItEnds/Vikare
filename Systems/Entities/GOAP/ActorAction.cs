using System;
using System.Collections.Generic;
using Vikare.Entities.GOAP.Strategies;

namespace Vikare.Entities.GOAP
{
    /// <summary> A potential action an entity can use to try to address a goal. </summary>
    public class ActorAction : IEquatable<ActorAction>
    {
        /// <summary> The identifying key used for equality, hashing, and planner deduplication. </summary>
        public String Name { get; }

        /// <summary> Returns the action's current planning cost; higher means less preferred. </summary>
        public Func<Single> Cost { get; private set; } = () => 1f;

        /// <summary> Facts that must evaluate true in the world state for this action to be eligible. </summary>
        public HashSet<ActorFact> Preconditions { get; } = new HashSet<ActorFact>();

        /// <summary> Facts the planner treats as true after this action completes; used for backward-chaining matching. </summary>
        public HashSet<ActorFact> Outcomes { get; } = new HashSet<ActorFact>();

        /// <summary> Whether the action's strategy has run to completion. </summary>
        public Boolean IsComplete => _strategy.IsComplete;

        /// <summary> Whether the underlying strategy is still viable; a false reading means the strategy aborted mid-run and the action must be torn down rather than awaited. </summary>
        public Boolean IsValid => _strategy.IsValid;


        /// <summary> The strategy that drives this action's execution. </summary>
        private readonly IActionStrategy _strategy;


        /// <summary> Creates a named action backed by the given execution strategy. </summary>
        /// <param name="name"> The identifying key for this action. </param>
        /// <param name="strategy"> The strategy that drives execution. </param>
        private ActorAction(String name, IActionStrategy strategy)
        {
            Name = name;
            _strategy = strategy;
        }


        /// <summary> Starts the action's underlying strategy. </summary>
        public void Start() => _strategy.Start();


        /// <summary> Advances the action's strategy by one tick; delegates entirely to the strategy. </summary>
        /// <param name="delta"> Time elapsed since the last update, in seconds. </param>
        public void Update(Double delta)
        {
            if (_strategy.IsValid)
            {
                _strategy.Update(delta);
            }
        }


        /// <summary> Stops or cancels the action's underlying strategy gracefully. </summary>
        public void Stop() => _strategy.Stop();


        /// <inheritdoc/>
        public override Int32 GetHashCode() => HashCode.Combine(Name);


        /// <inheritdoc/>
        public override Boolean Equals(Object? obj)
        {
            ActorAction? other = obj as ActorAction;
            return other != null && Name.Equals(other.Name, StringComparison.Ordinal);
        }


        /// <inheritdoc/>
        public Boolean Equals(ActorAction? other) => other != null && Name.Equals(other.Name, StringComparison.Ordinal);


        /// <summary> A builder for constructing actions before registration with the controller. </summary>
        public class Builder
        {
            /// <summary> The action under construction. </summary>
            private readonly ActorAction _action;


            /// <summary> Creates a builder for an action with the given name and execution strategy. </summary>
            /// <param name="name"> The identifying key for this action. </param>
            /// <param name="strategy"> The strategy that drives execution. </param>
            public Builder(String name, IActionStrategy strategy)
            {
                _action = new ActorAction(name, strategy);
            }


            /// <summary> Sets a dynamic cost function evaluated during plan search. </summary>
            /// <param name="cost"> Delegate returning the action's current cost. </param>
            public Builder WithCost(Func<Single> cost)
            {
                _action.Cost = cost;
                return this;
            }


            /// <summary> Sets a fixed planning cost. </summary>
            /// <param name="cost"> The constant cost assigned to this action. </param>
            public Builder WithCost(Single cost)
            {
                _action.Cost = () => cost;
                return this;
            }


            /// <summary> Sets cost as the Euclidean distance between two entities, evaluated at planning time. </summary>
            /// <param name="actor"> The actor performing the action. </param>
            /// <param name="other"> The entity to measure distance to. </param>
            public Builder WithDistanceCost(Actor actor, Entity other)
            {
                _action.Cost = () => actor.GlobalPosition.DistanceTo(other.GlobalPosition);
                return this;
            }


            /// <summary> Adds a fact that must evaluate true for this action to be eligible. </summary>
            /// <param name="precondition"> The fact that must hold before this action may be taken. </param>
            public Builder AddPrecondition(ActorFact precondition)
            {
                _action.Preconditions.Add(precondition);
                return this;
            }


            /// <summary> Adds multiple facts that must all evaluate true for this action to be eligible. </summary>
            /// <param name="preconditions"> The facts that must all hold before this action may be taken. </param>
            public Builder AddPrecondition(ActorFact[] preconditions)
            {
                foreach (ActorFact precondition in preconditions)
                {
                    _action.Preconditions.Add(precondition);
                }

                return this;
            }


            /// <summary> Declares a fact the planner treats as true after this action completes. </summary>
            /// <param name="outcome"> The fact the planner uses to match this action against pending requirements. </param>
            public Builder AddOutcome(ActorFact outcome)
            {
                _action.Outcomes.Add(outcome);
                return this;
            }


            /// <summary> Declares multiple facts the planner treats as true after this action completes. </summary>
            /// <param name="outcomes"> The facts used to match this action against pending requirements. </param>
            public Builder AddOutcome(ActorFact[] outcomes)
            {
                foreach (ActorFact outcome in outcomes)
                {
                    _action.Outcomes.Add(outcome);
                }

                return this;
            }


            /// <summary> Returns the configured action. </summary>
            /// <returns> The newly constructed action, ready for registration. </returns>
            public ActorAction Build()
            {
                return _action;
            }
        }
    }
}
