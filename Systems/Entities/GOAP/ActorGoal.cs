using System;
using System.Collections.Generic;

namespace Vikare.Entities.GOAP
{
    /// <summary> A goal that the actor desires and will use a chain of actions to complete. </summary>
    /// <remarks> Under the hood this is really just a series of desired states / facts. </remarks>
    public class ActorGoal : IEquatable<ActorGoal>
    {
        /// <summary> The name or key identifying the goal. </summary>
        public String Name { get; }

        /// <summary> The importance of the goal the actor. More important goals will be tackled before less. </summary>
        public GoalPriority Priority { get; private set; } = GoalPriority.NONE;

        /// <summary> The desired state of the world for the goal to be considered complete. All actions should go towards addressing these outcomes. </summary>
        public HashSet<ActorFact> DesiredOutcomes { get; } = new HashSet<ActorFact>();

        /// <summary> Where the goal originates from. </summary>
        public GoalSource Source { get; private set; }


        /// <summary> A goal that the actor desires and will use a chain of actions to complete. </summary>
        /// <param name="name"> The name or key identifying the goal. </param>
        /// <param name="source"> Where the goal originates from. </param>
        private ActorGoal(String name, GoalSource source)
        {
            Name = name;
            Source = source;
        }


        /// <summary> Update the priority of the goal. </summary>
        /// <param name="priority"> The goal's new priority. </param>
        public void UpdatePriority(GoalPriority priority)
        {
            Priority = priority;
        }


        /// <inheritdoc/>
        public override Int32 GetHashCode() => HashCode.Combine(Name);


        /// <inheritdoc/>
        public override Boolean Equals(Object? obj)
        {
            ActorGoal? other = obj as ActorGoal;
            return other != null ? Name.Equals(other.Name) : false;
        }


        /// <inheritdoc/>
        public bool Equals(ActorGoal? other) => other != null ? Name.Equals(other.Name) : false;


        /// <summary> A helpful builder that allows for easy construction of actor goals. </summary>
        public class Builder
        {
            /// <summary> A reference to the goal being constructed. </summary>
            private readonly ActorGoal _goal;


            /// <summary> A helpful builder that allows for easy construction of actor goals. </summary>
            /// <param name="name"> The name or key identifying the goal. </param>
            /// <param name="source"> Where the goal originates from. </param>
            public Builder(String name, GoalSource source)
            {
                _goal = new ActorGoal(name, source);
            }


            /// <summary> Sets the goal's priority. </summary>
            /// <param name="priority"> The importance of the goal the actor. More important goals will be tackled before less. </param>
            public Builder WithPriority(GoalPriority priority)
            {
                _goal.Priority = priority;
                return this;
            }


            /// <summary> Adds a desired outcome to the goal. </summary>
            /// <param name="outcome"> The desired state of the world for the goal to be considered complete. </param>
            public Builder WithDesiredOutcome(ActorFact outcome)
            {
                _goal.DesiredOutcomes.Add(outcome);
                return this;
            }


            /// <summary> Adds a desired outcome to the goal. </summary>
            /// <param name="outcomes"> The desired states of the world for the goal to be considered complete. </param>
            public Builder WithDesiredOutcome(ActorFact[] outcomes)
            {
                foreach (ActorFact outcome in outcomes)
                {
                    _goal.DesiredOutcomes.Add(outcome);
                }
                return this;
            }


            /// <summary> Build the architected goal. </summary>
            /// <returns> The newly constructed goal. </returns>
            public ActorGoal Build()
            {
                return _goal;
            }
        }
    }


    /// <summary> Where the goal originates from. </summary>
    public enum GoalSource
    {
        BASIC,  // Basic upkeep such as eating.
        PERSONAL,   // Personal goals related to the actor's desires.
        ORGANISATION    // Goals given by the organisation controlling the actor.
    }


    /// <summary> How important the goal is to the actor. </summary>
    public enum GoalPriority
    {
        NONE = 0,
        LOW = 1,
        MEDIUM = 2,
        HIGH = 3,
        CRITICAL = 4,   // Needs that relate to staying alive.
        OVERRIDE = 5
    }
}
