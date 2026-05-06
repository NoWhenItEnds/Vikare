using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Vikare.Entities.Components;
using Vikare.Entities.GOAP.Strategies;
using Vikare.Utilities.Extensions;

namespace Vikare.Entities.GOAP
{
    /// <summary> Uses a GOAP implementation to control an entity. The AI brain that controls a unit. </summary>
    public class ActorController
    {
        /// <summary> The entity this controller is responsible for controlling. </summary>
        public Actor Actor { get; init; }

        /// <summary> The current goal the actor is trying to accomplish. </summary>
        public ActorGoal? CurrentGoal { get; private set; } = null;

        /// <summary> The current plan the actor is using to address its current goal. </summary>
        public ActionPlan? CurrentPlan { get; private set; } = null;

        /// <summary> The current action the actor is in the process of doing. </summary>
        public ActorAction? CurrentAction { get; private set; } = null;

        /// <summary> An ordered array of the previous goals the actor tried to accomplish. </summary>
        /// <remarks> [0] is the latest. [^1] is the oldest. </remarks>
        public ActorGoal[] PreviousGoals { get; private set; } = new ActorGoal[10];

        /// <summary> The 'truths' the actor knows. The beliefs it has about the world state. </summary>
        public readonly Dictionary<String, ActorFact> AvailableFacts = new Dictionary<String, ActorFact>();

        /// <summary> The goals that the actor will seek to address. </summary>
        public readonly HashSet<ActorGoal> AvailableGoals = new HashSet<ActorGoal>();

        /// <summary> The potential actions this actor has access to. </summary>
        public readonly HashSet<ActorAction> AvailableActions = new HashSet<ActorAction>();

        /// <summary> The goals that the organisation has given the actor. </summary>
        public readonly HashSet<ActorGoal> OrganisationGoals = new HashSet<ActorGoal>();


        /// <summary> A reference to the planner this controller will use. </summary>
        private readonly ActorPlanner _planner = new ActorPlanner();


        /// <summary> Uses a GOAP implementation to control an entity. The AI brain that controls a unit. </summary>
        /// <param name="actor"> The entity this controller is responsible for controlling. </param>
        public ActorController(Actor actor)
        {
            Actor = actor;

            Dictionary<String, ActorFact> basicFacts = BuildBasicFacts();
            ActorAction[] basicActions = BuildBasicActions();
            ActorGoal[] basicGoals = BuildBasicGoals();

            AvailableFacts = AvailableFacts.Add(basicFacts);
            AvailableActions.UnionWith(basicActions);
            AvailableGoals.UnionWith(basicGoals);
        }


        /// <summary> Build's the agent's initial facts relating to basic upkeep. </summary>
        /// <returns> A set containing the constructed facts. </returns>
        private Dictionary<String, ActorFact> BuildBasicFacts()
        {
            FactFactory factory = new FactFactory(Actor);

            factory.AddFact("nothing", () => false);  // Always has a belief, even if it never will successfully evaluate.

            NeedsComponent? needsComponent = Actor.GetComponent<NeedsComponent>();
            if (needsComponent != null)
            {
                factory.AddFact("is_fresh", () => needsComponent.Stamina.Percent >= 0.9f);
                factory.AddFact("is_tired", () => needsComponent.Stamina.Percent < 0.5f);
                factory.AddFact("is_entertained", () => needsComponent.Entertainment.Percent >= 0.9f);
                factory.AddFact("is_bored", () => needsComponent.Entertainment.Percent < 0.5f);
            }

            return factory.Build();
        }


        /// <summary> Build's the agent's initial actions relating to basic upkeep. </summary>
        /// <returns> A set containing the constructed actions. </returns>
        private ActorAction[] BuildBasicActions()
        {
            HashSet<ActorAction> actions = new HashSet<ActorAction>();

            actions.Add(new ActorAction.Builder("Relax", new IdleStrategy(Actor, 5f))
                .AddOutcome(AvailableFacts["nothing"])
                .Build());

            actions.Add(new ActorAction.Builder("Wander", new WanderStrategy(Actor))
                .WithCost(() => 10f)    // TODO - Have calculated from actor personality.
                .AddOutcome(AvailableFacts["is_entertained"])
                .Build());

            return actions.ToArray();
        }


        /// <summary> Build's the agent's initial goals relating to basic upkeep. </summary>
        /// <returns> A set containing the constructed goals. </returns>
        private ActorGoal[] BuildBasicGoals()
        {
            HashSet<ActorGoal> goals = new HashSet<ActorGoal>();

            goals.Add(new ActorGoal.Builder("WatchPaintDry", GoalSource.BASIC)
                .WithPriority(GoalPriority.NONE)
                .WithDesiredOutcome(AvailableFacts["nothing"])
                .Build());

            goals.Add(new ActorGoal.Builder("KeepEntertained", GoalSource.BASIC)
                .WithPriority(GoalPriority.CRITICAL)
                .WithDesiredOutcome(AvailableFacts["is_entertained"])
                .Build());

            return goals.ToArray();
        }


        /// <summary> Force a hard reset of the current plan. </summary>
        public void ReevaluatePlan()
        {
            // Remove the current objective to force the planner to reevaluate.
            CurrentAction = null;
            ArchiveCurrentGoal();
        }


        /// <summary> Process the actor's plan. </summary>
        /// <param name="delta"> The time since the previous 'frame' this method was called. </param>
        public void ProcessPlan(Double delta)
        {
            // Update the plan and current action if there is one
            if (CurrentAction == null)
            {
                GD.Print($"{Actor.Name} -> Calculating new plan...");
                CalculatePlan();

                if (CurrentPlan != null && CurrentPlan.Actions.Count > 0)
                {
                    CurrentGoal = CurrentPlan.ActorGoal;
                    GD.Print($"{Actor.Name} -> Goal: {CurrentGoal.Name} with {CurrentPlan.Actions.Count} actions in plan.");

                    CurrentAction = CurrentPlan.Actions.Pop();
                    GD.Print($"{Actor.Name} -> Popped action: {CurrentAction.Name}.");

                    // Verify all precondition effects are true
                    if (CurrentAction.Preconditions.All(b => b.Evaluate()))
                    {
                        CurrentAction.Start();
                    }
                    else
                    {
                        GD.Print($"{Actor.Name} -> Goal preconditions not met, clearing current action and goal.");

                        CurrentAction = null;
                        CurrentGoal = null;
                    }
                }
            }


            // If we have a current action, execute it
            if (CurrentPlan != null && CurrentAction != null)
            {
                CurrentAction.Update(delta);

                if (CurrentAction.IsComplete)
                {
                    GD.Print($"{Actor.Name} -> Action, {CurrentAction.Name}, complete.");

                    CurrentAction.Stop();
                    CurrentAction = null;

                    if (CurrentPlan.Actions.Count == 0)
                    {
                        GD.Print($"{Actor.Name} -> Plan complete!");

                        ArchiveCurrentGoal();
                    }
                }
            }
        }


        /// <summary> Attempt to calculate a new plan. </summary>
        private void CalculatePlan()
        {
            GoalPriority priorityLevel = CurrentGoal != null ? CurrentGoal.Priority : GoalPriority.NONE;

            HashSet<ActorGoal> goalsToCheck = AvailableGoals;

            // If we have a current goal, we only want to check goals with higher priority.
            if (CurrentGoal != null)
            {
                goalsToCheck = new HashSet<ActorGoal>(AvailableGoals.Where(g => g.Priority > priorityLevel));
            }

            ActionPlan? potentialPlan = _planner.BuildPlan(this, goalsToCheck, PreviousGoals[0]);
            if (potentialPlan != null)
            {
                CurrentPlan = potentialPlan;
            }
        }


        /// <summary> Adds the current goal to the array of previous goals. </summary>
        private void ArchiveCurrentGoal()
        {
            if(CurrentGoal != null)
            {
                ActorGoal[] newValues = new ActorGoal[10];
                newValues[0] = CurrentGoal;
                Array.Copy(PreviousGoals, 0, newValues, 1, PreviousGoals.Length - 1);
                PreviousGoals = newValues;
            }

            CurrentGoal = null;
        }
    }
}
