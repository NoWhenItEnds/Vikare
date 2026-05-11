using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Vikare.Entities.Components;
using Vikare.Entities.GOAP.Advertisers;
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
        /// <remarks> A non-null CurrentAction implies this is also non-null; code that clears one must clear the other to maintain that invariant. </remarks>
        public ActionPlan? CurrentPlan { get; private set; } = null;

        /// <summary> The current action the actor is in the process of doing. </summary>
        /// <remarks> A non-null value implies CurrentPlan is also non-null. </remarks>
        public ActorAction? CurrentAction { get; private set; } = null;

        /// <summary> An ordered array of the previous goals the actor tried to accomplish. </summary>
        /// <remarks> [0] is the latest. [^1] is the oldest. </remarks>
        public ActorGoal[] PreviousGoals { get; private set; } = new ActorGoal[10];

        /// <summary> The truths the actor knows about the world — keyed by fact name. </summary>
        public readonly Dictionary<String, ActorFact> AvailableFacts = new Dictionary<String, ActorFact>();

        /// <summary> The goals the actor will seek to address. </summary>
        public readonly HashSet<ActorGoal> AvailableGoals = new HashSet<ActorGoal>();

        /// <summary> The potential actions this actor has access to. </summary>
        public readonly HashSet<ActorAction> AvailableActions = new HashSet<ActorAction>();

        /// <summary> The goals assigned by the actor's organisation. </summary>
        public readonly HashSet<ActorGoal> OrganisationGoals = new HashSet<ActorGoal>();


        /// <summary> Advertisers this actor knows about. </summary>
        /// <remarks> Empty by default. Call <see cref="LearnAdvertiser"/> to populate from a sensor or other learning source. </remarks>
        private readonly HashSet<ActionAdvertiser> _knownAdvertisers = new HashSet<ActionAdvertiser>();   // TODO - Implement.

        /// <summary> The planner used to build action sequences. </summary>
        private readonly ActorPlanner _planner = new ActorPlanner();

        /// <summary> When true, emits trace logs for plan and action transitions. </summary>
        private Boolean _useLogging = false;

        /// <summary>
        /// The factor by which a candidate goal's utility must exceed the current goal's utility
        /// to trigger an interrupt; prevents jitter when two goals have near-equal scores.
        /// </summary>
        private const Single _utilityInterruptMargin = 1.1f;    // TODO - Move to own class.

        /// <summary>
        /// Ceiling for drive-insistent utility functions; entertainment and hydration both scale
        /// against this value so they compete on equal footing.
        /// Formula: <c>MaxDriveUtility * (1f - need.Percent)</c>.
        /// </summary>
        private const Single MaxDriveUtility = 100f;

        /// <summary>
        /// Utility for the no-op fallback goal; zero ensures it is picked only when no other
        /// goal is in scope.
        /// </summary>
        private const Single WatchPaintDryUtility = 0f;


        /// <summary> Uses a GOAP implementation to control an entity. The AI brain that controls a unit. </summary>
        /// <param name="actor"> The entity this controller is responsible for controlling. </param>
        /// <param name="useLogging"> Whether the controller should log it's processes. </param>
        public ActorController(Actor actor, Boolean useLogging = false)
        {
            Actor = actor;
            _useLogging = useLogging;

            AvailableFacts = AvailableFacts.Add(BuildBasicFacts());
            AvailableActions.UnionWith(BuildBasicActions());
            AvailableGoals.UnionWith(BuildBasicGoals());
        }


        /// <summary> Forces a hard reset of the current plan so the next ProcessPlan call starts a fresh planning cycle. </summary>
        public void ReevaluatePlan()
        {
            ClearCurrentPlanState();
            ArchiveCurrentGoal();
        }


        /// <summary> Clears the current action and plan together, preserving the non-null invariant between them. </summary>
        private void ClearCurrentPlanState()
        {
            CurrentAction = null;
            CurrentPlan = null;
        }


        /// <summary> Processes the actor's plan for one tick. </summary>
        /// <param name="delta"> Time elapsed since the previous tick, in seconds. </param>
        public void ProcessPlan(Double delta)
        {
            if (CurrentAction == null)
            {
                if (_useLogging) { GD.Print($"{Actor.Name} -> Calculating new plan..."); }
                CalculatePlan();

                if (CurrentPlan != null && CurrentPlan.Actions.Count > 0)
                {
                    CurrentGoal = CurrentPlan.ActorGoal;
                    if (_useLogging) { GD.Print($"{Actor.Name} -> Goal: {CurrentGoal.Name} with {CurrentPlan.Actions.Count} actions in plan."); }

                    CurrentAction = CurrentPlan.Actions.Pop();
                    if (_useLogging) { GD.Print($"{Actor.Name} -> Popped action: {CurrentAction.Name}."); }

                    Boolean preconditionsMet = CurrentAction.Preconditions.All(precondition => precondition.Evaluate());

                    if (preconditionsMet)
                    {
                        CurrentAction.Start();
                    }
                    else
                    {
                        if (_useLogging) { GD.Print($"{Actor.Name} -> Goal preconditions not met, clearing current action and goal."); }

                        ClearCurrentPlanState();
                        CurrentGoal = null;
                    }
                }
            }

            if (CurrentPlan != null && CurrentAction != null)
            {
                CurrentAction.Update(delta);

                if (CurrentAction.IsComplete)
                {
                    if (_useLogging) { GD.Print($"{Actor.Name} -> Action, {CurrentAction.Name}, complete."); }

                    CurrentAction.Stop();
                    CurrentAction = null;

                    if (CurrentPlan.Actions.Count == 0)
                    {
                        if (_useLogging) { GD.Print($"{Actor.Name} -> Plan complete!"); }

                        ArchiveCurrentGoal();
                    }
                }
            }
        }


        /// <summary> Attempts to calculate a new plan. When a goal is already active, only goals that exceed its utility by at least <see cref="_utilityInterruptMargin"/> are considered, preventing jitter. </summary>
        private void CalculatePlan()
        {
            HashSet<ActorGoal> goalsToCheck;

            if (CurrentGoal != null)
            {
                Single currentUtility = CurrentGoal.Utility();
                goalsToCheck = new HashSet<ActorGoal>(AvailableGoals.Where(g => g.Utility() > currentUtility * _utilityInterruptMargin));
            }
            else
            {
                goalsToCheck = AvailableGoals;
            }

            HashSet<ActorAction> actions = BuildActionSetForPlanning();
            ActionPlan? potentialPlan = _planner.BuildPlan(goalsToCheck, actions);

            CurrentPlan = potentialPlan;
        }


        /// <summary> Builds the full action set for one planning round by unioning static actions with any actions advertised by world objects this actor knows about. </summary>
        /// <returns> A new set containing all planning-eligible actions for this round. </returns>
        private HashSet<ActorAction> BuildActionSetForPlanning()
        {
            HashSet<ActorAction> actions = new HashSet<ActorAction>(AvailableActions);

            foreach (ActionAdvertiser advertiser in _knownAdvertisers)
            {
                foreach (ActorAction advertisedAction in advertiser.GetAdvertisedActions(this))
                {
                    actions.Add(advertisedAction);
                }
            }

            return actions;
        }


        /// <summary> Moves the current goal to the front of <see cref="PreviousGoals"/> and clears it. </summary>
        private void ArchiveCurrentGoal()
        {
            if (CurrentGoal != null)
            {
                ActorGoal[] newValues = new ActorGoal[10];
                newValues[0] = CurrentGoal;
                Array.Copy(PreviousGoals, 0, newValues, 1, PreviousGoals.Length - 1);
                PreviousGoals = newValues;
            }

            CurrentGoal = null;
        }


        /// <summary> Build's the agent's initial facts relating to basic upkeep. </summary>
        /// <returns> A set containing the constructed facts. </returns>
        private Dictionary<String, ActorFact> BuildBasicFacts()
        {
            Dictionary<String, ActorFact> facts = new Dictionary<String, ActorFact>();

            // Always has a belief, even if it never will successfully evaluate.
            facts.Add("nothing", new ActorFact.Builder("nothing")
                .WithCondition(() => false)
                .Build());

            NeedsComponent? needsComponent = Actor.GetComponent<NeedsComponent>();
            if (needsComponent != null)
            {
                facts.Add("is_fresh", new ActorFact.Builder("is_fresh")
                    .WithCondition(() => needsComponent.Stamina.Percent >= 0.9f)
                    .Build());

                facts.Add("is_tired", new ActorFact.Builder("is_tired")
                    .WithCondition(() => needsComponent.Stamina.Percent < 0.5f)
                    .Build());

                facts.Add("is_entertained", new ActorFact.Builder("is_entertained")
                    .WithCondition(() => needsComponent.Entertainment.Percent >= 0.9f)
                    .Build());

                facts.Add("is_bored", new ActorFact.Builder("is_bored")
                    .WithCondition(() => needsComponent.Entertainment.Percent < 0.5f)
                    .Build());

                facts.Add("is_hydrated", new ActorFact.Builder("is_hydrated")
                    .WithCondition(() => needsComponent.Hydration.Percent >= 0.9f)
                    .Build());

                facts.Add("is_dehydrated", new ActorFact.Builder("is_dehydrated")
                    .WithCondition(() => needsComponent.Hydration.Percent < 0.5f)
                    .Build());
            }

            return facts;
        }


        /// <summary> Build's the agent's initial actions relating to basic upkeep. </summary>
        /// <returns> A set containing the constructed actions. </returns>
        private ActorAction[] BuildBasicActions()
        {
            HashSet<ActorAction> actions = new HashSet<ActorAction>();

            ActorFact nothingFact = AvailableFacts["nothing"];

            actions.Add(new ActorAction.Builder("Relax", new IdleStrategy(Actor, 1f))   // TODO - Based off something?
                .AddOutcome(nothingFact)
                .Build());

            if (AvailableFacts.TryGetValue("is_entertained", out ActorFact? isEntertainedFact))
            {
                actions.Add(new ActorAction.Builder("Wander", new WanderStrategy(Actor))
                    .WithCost(() => 1f) // TODO - Based on distance?
                    .AddOutcome(isEntertainedFact)
                    .Build());
            }

            if (AvailableFacts.TryGetValue("is_tired", out ActorFact? isTiredFact))
            {
                actions.Add(new ActorAction.Builder("Wander", new GoToEntityStrategy(Actor))
                    .WithCost(() => 1f) // TODO - Based on distance?
                    .AddOutcome(isEntertainedFact)
                    .Build());

                actions.Add(new ActorAction.Builder("UseBed", new GoToEntityStrategy(Actor))
                    .AddPrecondition(AvailableFacts["at_bed"])
                    .WithCost(() => 1f) // TODO - Based on distance?
                    .AddOutcome(AvailableFacts["is_rested"])
                    .Build());
            }

            return actions.ToArray();
        }


        /// <summary> Build's the agent's initial goals relating to basic upkeep. </summary>
        /// <returns> A set containing the constructed goals. </returns>
        private ActorGoal[] BuildBasicGoals()
        {
            HashSet<ActorGoal> goals = new HashSet<ActorGoal>();

            goals.Add(new ActorGoal.Builder("WatchPaintDry", GoalSource.BASIC)
                .WithUtility(WatchPaintDryUtility)
                .WithDesiredOutcome(AvailableFacts["nothing"])
                .Build());

            goals.Add(new ActorGoal.Builder("KeepEntertained", GoalSource.BASIC)
                .WithUtility(MaxDriveUtility)
                .WithDesiredOutcome(AvailableFacts["is_entertained"])
                .Build());

            goals.Add(new ActorGoal.Builder("StayHydrated", GoalSource.BASIC)
                .WithUtility(MaxDriveUtility)
                .WithDesiredOutcome(AvailableFacts["is_hydrated"])
                .Build());

            return goals.ToArray();
        }
    }
}
