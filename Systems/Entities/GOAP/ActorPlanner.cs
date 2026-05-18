using System;
using System.Collections.Generic;
using System.Linq;

namespace Vikare.Entities.GOAP
{
    /// <summary> Handles the logic of constructing a method to reach a desired goal. </summary>
    /// <remarks> https://www.youtube.com/watch?v=T_sBYgP7_2k </remarks>
    public class ActorPlanner
    {
        /// <summary> Maximum recursion depth for plan search; guards against precondition cycles. </summary>
        private const Int32 _maxPlanDepth = 32;


        /// <summary> Attempts to build a plan for the first satisfiable goal in the provided sequence. Runs a DFS through the action graph for each goal in order, returning the first satisfiable plan found. </summary>
        /// <param name="orderedGoals"> The goals to plan for, in the order the caller wants them attempted. The planner takes the first one whose plan is satisfiable. </param>
        /// <param name="actions"> The full action set available this planning round, including advertised actions. </param>
        /// <returns> The constructed plan, or null if no satisfiable plan was found. </returns>
        /// <remarks> A <see cref="PlanningState"/> snapshot is constructed once per call so every goal attempt and DFS branch sees a consistent world view. </remarks>
        public ActionPlan? BuildPlan(IEnumerable<ActorGoal> orderedGoals, HashSet<ActorAction> actions)
        {
            PlanningState state = new PlanningState();

            ActionPlan? result = null;

            foreach (ActorGoal goal in orderedGoals.Where(g => !state.SatisfiedBy(g.DesiredOutcomes)))
            {
                if (result == null)
                {
                    GraphNode goalNode = new GraphNode(null, null, goal.DesiredOutcomes, 0);

                    List<GraphNode> terminals = new List<GraphNode>();
                    FindPath(goalNode, actions, terminals, 0, state);

                    if (terminals.Count > 0)
                    {
                        GraphNode cheapestTerminal = terminals.OrderBy(n => n.Cost).First();
                        Queue<ActorAction> actionQueue = ReconstructPath(cheapestTerminal);
                        result = new ActionPlan(goal, actionQueue, cheapestTerminal.Cost);
                    }
                }
            }

            return result;
        }


        /// <summary> Walks parent pointers from a terminal node up to the root, returning a queue whose head is the first action to execute. </summary>
        /// <param name="terminal"> The fully-satisfied leaf node to reconstruct from. </param>
        /// <returns> A queue ordered so that <see cref="Queue{T}.Dequeue"/> returns actions in execution order. </returns>
        private Queue<ActorAction> ReconstructPath(GraphNode terminal)
        {
            Queue<ActorAction> actionQueue = new Queue<ActorAction>();
            GraphNode? current = terminal;

            while (current != null && current.Action != null)
            {
                actionQueue.Enqueue(current.Action);
                current = current.Parent;
            }

            return actionQueue;
        }


        /// <summary> Performs a depth-first search from <paramref name="parent"/>, expanding actions whose outcomes address at least one pending required fact. Terminal nodes — those with no remaining unsatisfied requirements — are appended to <paramref name="terminals"/>. </summary>
        /// <param name="parent"> The node to expand from. </param>
        /// <param name="actions"> The full action set available this planning round. </param>
        /// <param name="terminals"> Accumulated terminal nodes; caller picks the cheapest after the full DFS. </param>
        /// <param name="depth"> Current recursion depth; bails out at <see cref="_maxPlanDepth"/>. </param>
        /// <param name="state"> The world-state snapshot for this planning pass. </param>
        /// <returns> True if at least one satisfiable branch was found beneath this node. </returns>
        /// <remarks> The parent node's required set is never mutated. </remarks>
        private Boolean FindPath(GraphNode parent, HashSet<ActorAction> actions, List<GraphNode> terminals, Int32 depth, PlanningState state)
        {
            Boolean pathFound;

            if (depth >= _maxPlanDepth)
            {
                pathFound = false;
            }
            else
            {
                HashSet<ActorFact> pendingFacts = new HashSet<ActorFact>(
                    parent.RequiredFacts.Where(f => !state.Get(f)));

                if (pendingFacts.Count == 0)
                {
                    terminals.Add(parent);
                    pathFound = true;
                }
                else
                {
                    IOrderedEnumerable<ActorAction> orderedActions = actions.OrderBy(a => a.Cost());

                    pathFound = false;

                    foreach (ActorAction action in orderedActions)
                    {
                        HashSet<ActorFact> satisfiedByAction = new HashSet<ActorFact>(
                            pendingFacts.Where(f => action.Outcomes.Contains(f)));

                        Boolean actionAddressesRequirement = satisfiedByAction.Count > 0;

                        if (actionAddressesRequirement)
                        {
                            HashSet<ActorFact> newRequired = new HashSet<ActorFact>(pendingFacts);
                            newRequired.ExceptWith(satisfiedByAction);
                            newRequired.UnionWith(action.Preconditions);

                            GraphNode newNode = new GraphNode(parent, action, newRequired, parent.Cost + action.Cost());

                            Boolean branchFound = FindPath(newNode, actions, terminals, depth + 1, state);

                            if (branchFound)
                            {
                                parent.Leaves.Add(newNode);
                                pathFound = true;
                            }
                        }
                    }
                }
            }

            return pathFound;
        }
    }


    /// <summary> A goal paired with the ordered action sequence required to satisfy it. </summary>
    public class ActionPlan
    {
        /// <summary> The goal this plan is attempting to satisfy. </summary>
        public ActorGoal ActorGoal { get; }

        /// <summary> Read-only view of the remaining actions; exposes <c>Count</c> and enumeration but hides mutation. </summary>
        public IReadOnlyCollection<ActorAction> Actions => _actions;

        /// <summary> The sum of all action costs in the plan. </summary>
        public Single TotalCost { get; set; }

        /// <summary> The backing queue; private so only <see cref="DequeueNext"/> can remove actions. </summary>
        private readonly Queue<ActorAction> _actions;


        /// <summary> Creates a plan pairing a goal with its ordered action sequence and total cost. </summary>
        /// <param name="goal"> The goal this plan is attempting to satisfy. </param>
        /// <param name="actions"> The ordered actions required to satisfy the goal. The caller must not retain a reference after construction. </param>
        /// <param name="totalCost"> The sum of all action costs in the plan. </param>
        public ActionPlan(ActorGoal goal, Queue<ActorAction> actions, Single totalCost)
        {
            ActorGoal = goal;
            _actions = actions;
            TotalCost = totalCost;
        }


        /// <summary> Removes and returns the next action in execution order, reducing <see cref="Actions"/>.<c>Count</c> by one. </summary>
        /// <returns> The next <see cref="ActorAction"/> to execute. </returns>
        /// <exception cref="InvalidOperationException"> Thrown by the underlying queue when <see cref="Actions"/> is empty. </exception>
        public ActorAction DequeueNext()
        {
            return _actions.Dequeue();
        }
    }


    /// <summary> A node in the backward-chaining planning graph. </summary>
    public class GraphNode
    {
        /// <summary> This node's parent in the planning graph. </summary>
        public GraphNode? Parent { get; }

        /// <summary> The action this node represents. </summary>
        public ActorAction? Action { get; }

        /// <summary> The facts still required to be true at this point in the graph. </summary>
        /// <remarks> Visitors must copy before mutating — <c>FindPath</c> relies on the parent's set being untouched. </remarks>
        public HashSet<ActorFact> RequiredFacts { get; }

        /// <summary> Child nodes for which a satisfiable subpath was confirmed. </summary>
        public List<GraphNode> Leaves { get; }

        /// <summary> Accumulated cost from the root to this node. </summary>
        public Single Cost { get; }

        /// <summary> True when this node has no leaves and no action — a dead branch. </summary>
        public Boolean IsLeafDead => Leaves.Count == 0 && Action == null;


        /// <summary> Creates a graph node with a given parent, action, pending requirements, and accumulated cost. </summary>
        /// <param name="parent"> This node's parent in the planning graph. </param>
        /// <param name="action"> The action this node represents. </param>
        /// <param name="requiredFacts"> The facts still to be satisfied at this position in the graph. </param>
        /// <param name="cost"> Accumulated cost from the root to this node. </param>
        public GraphNode(GraphNode? parent, ActorAction? action, HashSet<ActorFact> requiredFacts, Single cost)
        {
            Parent = parent;
            Action = action;
            RequiredFacts = new HashSet<ActorFact>(requiredFacts);
            Leaves = new List<GraphNode>();
            Cost = cost;
        }
    }
}
