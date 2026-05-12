using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Vikare.Entities.Components;
using Vikare.Entities.GOAP.Advertisers;
using Vikare.Entities.GOAP.Strategies;

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

        /// <summary> The goals the actor will seek to address. </summary>
        public readonly HashSet<ActorGoal> AvailableGoals = new HashSet<ActorGoal>();

        /// <summary> The goals assigned by the actor's organisation. </summary>
        public readonly HashSet<ActorGoal> OrganisationGoals = new HashSet<ActorGoal>();


        /// <summary> Advertisers this actor knows about. Facts and actions are re-contributed each planning round so that forgotten advertisers are automatically pruned from the world-model. </summary>
        /// <remarks> Empty until a sensor or other learning source populates it. </remarks>
        private readonly HashSet<ActionAdvertiser> _knownAdvertisers = new HashSet<ActionAdvertiser>();   // TODO - Implement.

        /// <summary> The baseline facts built once at construction time; independent of any advertiser. </summary>
        private readonly Dictionary<String, ActorFact> _basicFacts;

        /// <summary> The baseline actions built once at construction time; independent of any advertiser. </summary>
        private readonly HashSet<ActorAction> _basicActions;

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
        /// <param name="useLogging"> Whether the controller should log its processes. </param>
        public ActorController(Actor actor, Boolean useLogging = false)
        {
            Actor = actor;
            _useLogging = useLogging;

            _basicFacts = new Dictionary<String, ActorFact>(BuildBasicFacts());
            _basicActions = new HashSet<ActorAction>(BuildBasicActions());
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


        /// <summary> Attempts to calculate a new plan. Rebuilds from scratch each round so that facts and actions from forgotten advertisers are not carried forward. </summary>
        /// <remarks> When a goal is already active, only goals that exceed its utility by at least <see cref="_utilityInterruptMargin"/> are considered, preventing jitter. </remarks>
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

            Dictionary<String, ActorFact> runtimeFacts = BuildRuntimeFacts();
            HashSet<ActorAction> runtimeActions = BuildRuntimeActions(runtimeFacts);
            ActionPlan? potentialPlan = _planner.BuildPlan(goalsToCheck, runtimeActions);

            CurrentPlan = potentialPlan;
        }


        /// <summary> Builds the current facts from the stable baseline facts plus any facts contributed by currently-known advertisers. </summary>
        /// <returns> The constructed facts. </returns>
        private Dictionary<String, ActorFact> BuildRuntimeFacts()
        {
            Dictionary<String, ActorFact> result = new Dictionary<string, ActorFact>(_basicFacts);

            foreach (ActionAdvertiser advertiser in _knownAdvertisers)
            {
                foreach (KeyValuePair<String, ActorFact> entry in advertiser.GetFacts(Actor))
                {
                    if (!result.TryAdd(entry.Key, entry.Value))
                    {
                        GD.PushError($"Advertiser {advertiser.GetType().Name} tried to contribute fact '{entry.Key}' but a fact with that key is already present.");
                    }
                }
            }

            return result;
        }


        /// <summary> Builds the full action set for one planning round by combining the actor's standing actions with any actions advertised by world objects currently known to this actor. </summary>
        /// <param name="runtimeFacts"> The current facts that exist at this point in runtime. </param>
        /// <returns> A new set containing all planning-eligible actions for this round. </returns>
        /// <remarks> Must be called after <see cref="BuildRuntimeFacts"/> so that advertiser-contributed facts are already present. </remarks>
        private HashSet<ActorAction> BuildRuntimeActions(Dictionary<String, ActorFact> runtimeFacts)
        {
            HashSet<ActorAction> actions = new HashSet<ActorAction>(_basicActions);

            foreach (ActionAdvertiser advertiser in _knownAdvertisers)
            {
                foreach (ActorAction action in advertiser.GetActions(Actor, runtimeFacts))
                {
                    if (!actions.Add(action))
                    {
                        GD.PushError($"Advertiser {advertiser.GetType().Name} tried to contribute action '{action.Name}' but an action with that type is already present.");
                    }
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


        /// <summary> Builds the actor's baseline facts — those that are always present regardless of which advertisers are known. </summary>
        /// <returns> A dictionary of baseline facts keyed by fact name. </returns>
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


        /// <summary> Builds the actor's standing actions — those that are always available regardless of perceived advertisers. </summary>
        /// <returns> A set containing the constructed actions. </returns>
        private ActorAction[] BuildBasicActions()
        {
            HashSet<ActorAction> actions = new HashSet<ActorAction>();

            ActorFact nothingFact = _basicFacts["nothing"];

            actions.Add(new ActorAction.Builder("Relax", new IdleStrategy(Actor, 1f))   // TODO - Based off something?
                .AddOutcome(nothingFact)
                .Build());

            if (_basicFacts.TryGetValue("is_entertained", out ActorFact? isEntertainedFact))
            {
                actions.Add(new ActorAction.Builder("Wander", new WanderStrategy(Actor))
                    .WithCost(() => 1f) // TODO - Based on distance?
                    .AddOutcome(isEntertainedFact)
                    .Build());
            }

            return actions.ToArray();
        }


        /// <summary> Builds the actor's standing goals — those that are always active regardless of perceived advertisers. </summary>
        /// <returns> A set containing the constructed goals. </returns>
        private ActorGoal[] BuildBasicGoals()
        {
            HashSet<ActorGoal> goals = new HashSet<ActorGoal>();

            goals.Add(new ActorGoal.Builder("WatchPaintDry", GoalSource.BASIC)
                .WithUtility(WatchPaintDryUtility)
                .WithDesiredOutcome(_basicFacts["nothing"])
                .Build());

            goals.Add(new ActorGoal.Builder("KeepEntertained", GoalSource.BASIC)
                .WithUtility(MaxDriveUtility)
                .WithDesiredOutcome(_basicFacts["is_entertained"])
                .Build());

            goals.Add(new ActorGoal.Builder("StayHydrated", GoalSource.BASIC)
                .WithUtility(MaxDriveUtility)
                .WithDesiredOutcome(_basicFacts["is_hydrated"])
                .Build());

            return goals.ToArray();
        }
    }
}
