using System;
using System.Collections.Generic;
using Vikare.Entities.GOAP.Strategies;

namespace Vikare.Entities.GOAP.Advertisers
{
    /// <summary> An object an actor can interact with to restore stamina — beds, chairs, hammocks, etc. </summary>
    public class StaminaRestoringAdvertiser : ActionAdvertiser
    {
        /// <summary> Squared interaction radius in world units; the actor is considered adjacent when its squared distance to the host falls within this threshold (equivalent to a 2-unit radius). </summary>
        private const Single _atRangeSquared = 4f;


        /// <summary> An object an actor can interact with to restore stamina — beds, chairs, hammocks, etc. </summary>
        /// <param name="host"> The entity at whose location the restore action will be offered. </param>
        public StaminaRestoringAdvertiser(Entity host) : base(host) { }


        /// <inheritdoc/>
        public override Dictionary<String, ActorFact> GetFacts(Actor actor)
        {
            Dictionary<String, ActorFact> facts = new Dictionary<String, ActorFact>();

            String factKey = $"at_{_hostId}";
            facts.Add(factKey, new ActorFact.Builder(factKey)
                .WithCondition(() => actor.GlobalPosition.DistanceSquaredTo(_host.GlobalPosition) <= _atRangeSquared)
                .Build());

            return facts;
        }


        /// <inheritdoc/>
        public override IEnumerable<ActorAction> GetActions(Actor actor, Dictionary<String, ActorFact> existingFacts)
        {
            List<ActorAction> actions = new List<ActorAction>();

            ActorFact atFact = existingFacts[$"at_{_hostId}"];

            ActorAction moveToAction = new ActorAction.Builder($"MoveTo_{_hostId}", new GoToEntityStrategy(actor, _host))
                .WithDistanceCost(actor, _host)
                .AddOutcome(atFact)
                .Build();

            actions.Add(moveToAction);

            if (existingFacts.TryGetValue("is_fresh", out ActorFact? isFreshFact))
            {
                ActorAction useAction = new ActorAction.Builder($"Use_{_hostId}", new IdleStrategy(actor, 1f))
                    .AddPrecondition(atFact)
                    .AddOutcome(isFreshFact)
                    .Build();

                actions.Add(useAction);
            }

            return actions;
        }
    }
}
