using System;
using System.Collections.Generic;

namespace Vikare.Entities.GOAP
{
    /// <summary> A goal the actor will seek to satisfy through a chain of planned actions. </summary>
    public class ActorGoal : IEquatable<ActorGoal>
    {
        /// <summary> The identifying key used for equality and hashing. </summary>
        public String Name { get; }

        /// <summary> Returns the goal's current attraction score; higher means more urgent. Re-evaluated each planning round. </summary>
        public Func<Single> Utility { get; private set; } = () => 0f;

        /// <summary> The facts that must all evaluate to true for the goal to be considered satisfied. </summary>
        public HashSet<ActorFact> DesiredOutcomes { get; } = new HashSet<ActorFact>();

        /// <summary> Where the goal originates from. </summary>
        public GoalSource Source { get; private set; }


        /// <summary> Creates a named goal with a given source. </summary>
        /// <param name="name"> The identifying key for this goal. </param>
        /// <param name="source"> Where the goal originates from. </param>
        private ActorGoal(String name, GoalSource source)
        {
            Name = name;
            Source = source;
        }


        /// <inheritdoc/>
        public override Int32 GetHashCode() => HashCode.Combine(Name);


        /// <inheritdoc/>
        public override Boolean Equals(Object? obj)
        {
            ActorGoal? other = obj as ActorGoal;
            return other != null && Name.Equals(other.Name, StringComparison.Ordinal);
        }


        /// <inheritdoc/>
        public Boolean Equals(ActorGoal? other) => other != null && Name.Equals(other.Name, StringComparison.Ordinal);


        /// <summary> A builder for constructing goals before registration with the controller. </summary>
        public class Builder
        {
            /// <summary> The goal under construction. </summary>
            private readonly ActorGoal _goal;


            /// <summary> Creates a builder for a goal with the given name and source. </summary>
            /// <param name="name"> The identifying key for this goal. </param>
            /// <param name="source"> Where the goal originates from. </param>
            public Builder(String name, GoalSource source)
            {
                _goal = new ActorGoal(name, source);
            }


            /// <summary> Sets a dynamic utility function evaluated each planning round. </summary>
            /// <param name="utility"> Delegate returning the goal's current score; higher is more attractive. </param>
            public Builder WithUtility(Func<Single> utility)
            {
                _goal.Utility = utility;
                return this;
            }


            /// <summary> Sets a fixed utility score, wrapped in a closure. </summary>
            /// <param name="utility"> Constant utility score; higher means more attractive. </param>
            public Builder WithUtility(Single utility)
            {
                _goal.Utility = () => utility;
                return this;
            }


            /// <summary> Adds a single fact that must evaluate true for the goal to be satisfied. </summary>
            /// <param name="outcome"> The fact the world must make true. </param>
            public Builder WithDesiredOutcome(ActorFact outcome)
            {
                _goal.DesiredOutcomes.Add(outcome);
                return this;
            }


            /// <summary> Adds multiple facts that must all evaluate true for the goal to be satisfied. </summary>
            /// <param name="outcomes"> The facts the world must make true. </param>
            public Builder WithDesiredOutcome(ActorFact[] outcomes)
            {
                foreach (ActorFact outcome in outcomes)
                {
                    _goal.DesiredOutcomes.Add(outcome);
                }

                return this;
            }


            /// <summary> Returns the configured goal. </summary>
            /// <returns> The newly constructed goal, ready for registration. </returns>
            public ActorGoal Build()
            {
                return _goal;
            }
        }
    }


    /// <summary> Where the goal originates from. </summary>
    public enum GoalSource
    {
        /// <summary> Basic upkeep goals such as eating or resting. </summary>
        BASIC,
        /// <summary> Personal goals related to the actor's individual desires. </summary>
        PERSONAL,
        /// <summary> Goals given by the organisation controlling the actor. </summary>
        ORGANISATION
    }
}
