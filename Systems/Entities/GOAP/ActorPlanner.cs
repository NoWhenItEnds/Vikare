using System;
using System.Collections.Generic;
using System.Linq;

namespace Vikare.Entities.GOAP
{
    /// <summary> Handles the logic of constructing a method to reach a desired goal. </summary>
    /// <remarks> https://www.youtube.com/watch?v=T_sBYgP7_2k </remarks>
    public class ActorPlanner
    {
        /// <summary> Attempt to build a plan to address the actor's highest priority goal. </summary>
        /// <param name="actor"> A reference to the actor for whom this plan is for. </param>
        /// <param name="goals"> The goals to plan for. </param>
        /// <param name="mostRecentGoal"> A reference to the most recent goal the actor attempted to address. </param>
        /// <returns> The constructed plan. A null value means that we couldn't find one. </returns>
        public ActionPlan? BuildPlan(ActorController actor, HashSet<ActorGoal> goals, ActorGoal? mostRecentGoal = null)
        {
            // Order goals by priority, descending
            IOrderedEnumerable<ActorGoal> orderedGoals = goals
                .Where(g => g.DesiredOutcomes.Any(f => !f.Evaluate()))  // Don't include goals who's outcomes are already complete.
                .OrderByDescending(g => (Single)g.Priority);

            // Try to solve each goal in order
            foreach (ActorGoal goal in orderedGoals)
            {
                GraphNode goalNode = new GraphNode(null, null, goal.DesiredOutcomes, 0);

                // If we can find a path to the goal, return the plan
                if (FindPath(goalNode, actor.AvailableActions))
                {
                    // If the goalNode has no leaves and no action to perform try a different goal
                    if (!goalNode.IsLeafDead)
                    {
                        Stack<ActorAction> actionStack = new Stack<ActorAction>();
                        while (goalNode.Leaves.Count > 0)
                        {
                            GraphNode cheapestLeaf = goalNode.Leaves.OrderBy(leaf => leaf.Cost).First();
                            goalNode = cheapestLeaf;
                            if (cheapestLeaf.Action != null)
                            {
                                actionStack.Push(cheapestLeaf.Action);
                            }
                        }

                        return new ActionPlan(goal, actionStack, goalNode.Cost);
                    }
                }
            }

            return null;
        }


        /// <summary> Continue tracing a path from the parent using the available actions. </summary>
        /// <param name="parent"> A reference to the direction parent node we're pathing from. </param>
        /// <param name="actions"> The set of actions we have access to at this step. </param>
        /// <returns> Whether a path was successfully found at this level. </returns>
        private Boolean FindPath(GraphNode parent, HashSet<ActorAction> actions)
        {
            // Order actions by cost, ascending
            IOrderedEnumerable<ActorAction> orderedActions = actions.OrderBy(a => a.Cost());

            foreach (ActorAction action in orderedActions)
            {
                HashSet<ActorFact> requiredFacts = parent.RequiredFacts;

                // Remove any facts that evaluate to true, there is no action to take. They're already done.
                requiredFacts.RemoveWhere(f => f.Evaluate());

                // If there are no required facts to fulfill, we have a plan. No need to search further.
                if (requiredFacts.Count == 0)
                {
                    return true;
                }

                // If this action addresses any of the required outcomes, it's worth exploring.
                if (action.Outcomes.Any(requiredFacts.Contains))
                {
                    HashSet<ActorFact> newRequiredFacts = new HashSet<ActorFact>(requiredFacts);
                    newRequiredFacts.ExceptWith(action.Outcomes); // Remove any that have already been satisfied.
                    newRequiredFacts.UnionWith(action.Preconditions); // Add any preconditions that haven't been.

                    GraphNode newNode = new GraphNode(parent, action, newRequiredFacts, parent.Cost + action.Cost());

                    // Explore the new node, recursively.
                    if (FindPath(newNode, actions))
                    {
                        parent.Leaves.Add(newNode);
                        if (newNode.Action != null)
                        {
                            newRequiredFacts.ExceptWith(newNode.Action.Preconditions);
                        }

                    }

                    // If all effects at this depth have been satisfied, return true
                    if (newRequiredFacts.Count == 0)
                    {
                        return true;
                    }
                }
            }

            return parent.Leaves.Count > 0;
        }
    }


    /// <summary> A data structure for holding a desired goal and the actions needed to reach the goal. </summary>
    public class ActionPlan
    {
        /// <summary> The goal this plan is attempting to satisfy. </summary>
        public ActorGoal ActorGoal { get; }

        /// <summary> The ordered actions required to satisfy the goal. </summary>
        public Stack<ActorAction> Actions { get; }

        /// <summary> The plan's total cost. A sum of all the actions' costs. </summary>
        public Single TotalCost { get; set; }


        /// <summary> A data structure for holding a desired goal and the actions needed to reach the goal. </summary>
        /// <param name="goal"> The goal this plan is attempting to satisfy. </param>
        /// <param name="actions"> The ordered actions required to satisfy the goal. </param>
        /// <param name="totalCost"> The plan's total cost. A sum of all the actions' costs. </param>
        public ActionPlan(ActorGoal goal, Stack<ActorAction> actions, Single totalCost)
        {
            ActorGoal = goal;
            Actions = actions;
            TotalCost = totalCost;
        }
    }


    /// <summary> A node in a graph data structure. </summary>
    public class GraphNode
    {
        /// <summary> A reference to this node's parent. </summary>
        public GraphNode? Parent { get; }

        /// <summary> The action this node represents. </summary>
        public ActorAction? Action { get; }

        /// <summary> All the facts at THIS position in the graph. </summary>
        public HashSet<ActorFact> RequiredFacts { get; }

        /// <summary> References to all the children leaves. </summary>
        public List<GraphNode> Leaves { get; }

        /// <summary> A running cost of how expensive the graph is at this point. </summary>
        public Single Cost { get; }

        /// <summary> A node isn't worth considering if it has no children and no associated action. </summary>
        public Boolean IsLeafDead => Leaves.Count == 0 && Action == null;


        /// <summary> A node in a graph data structure. </summary>
        /// <param name="parent"> A reference to this node's parent. </param>
        /// <param name="action"> The action this node represents. </param>
        /// <param name="facts"> All the facts at THIS position in the graph. </param>
        /// <param name="cost"> A running cost of how expensive the graph is at this point. </param>
        public GraphNode(GraphNode? parent, ActorAction? action, HashSet<ActorFact> facts, Single cost)
        {
            Parent = parent;
            Action = action;
            RequiredFacts = new HashSet<ActorFact>(facts);    // We make a new set as there may be additional facts we need to satisfy as a result of our path.
            Leaves = new List<GraphNode>();
            Cost = cost;
        }
    }
}
