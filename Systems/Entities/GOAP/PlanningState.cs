using System;
using System.Collections.Generic;

namespace Vikare.Entities.GOAP
{
    /// <summary> A snapshot of fact truth values captured at the start of a single plan-search attempt. Provides a consistent view of the world for the duration of one <see cref="ActorPlanner.BuildPlan"/> </summary>
    /// <remarks> Facts are evaluated lazily on first access and cached by name-based identity. Cost functions continue to read from the live world on every call — this asymmetry is intentional; cost delegates are expected to be cheap and stateless. </remarks>
    public class PlanningState
    {
        /// <summary> Lazily populated cache mapping each fact to its evaluated result for this planning pass. </summary>
        private readonly Dictionary<ActorFact, Boolean> _cache = new Dictionary<ActorFact, Boolean>();


        /// <summary> Returns the cached truth value of a fact, evaluating it exactly once on first access. </summary>
        /// <param name="fact"> The fact to evaluate. </param>
        /// <returns> The fact's truth value as it was at the first query in this planning pass. </returns>
        public Boolean Get(ActorFact fact)
        {
            Boolean value;

            if (!_cache.TryGetValue(fact, out value))
            {
                value = fact.Evaluate();
                _cache[fact] = value;
            }

            return value;
        }


        /// <summary> Returns true when every fact in the iterable evaluates to true in this snapshot. </summary>
        /// <param name="facts"> The facts that must all be true. </param>
        /// <returns> True if all facts evaluate true; false if any evaluates false. </returns>
        public Boolean SatisfiedBy(IEnumerable<ActorFact> facts)
        {
            Boolean allTrue = true;

            foreach (ActorFact fact in facts)
            {
                if (!Get(fact))
                {
                    allTrue = false;
                }
            }

            return allTrue;
        }
    }
}
