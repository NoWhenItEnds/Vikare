using System.Collections.Generic;
using Vikare.Entities.GOAP.Strategies;

namespace Vikare.Entities.GOAP.Advertisers
{
    /// <summary> Allows an Actor to rest. Take a load off. Regain stamina. </summary>
    public class RestSpotAdvertiser : ActionAdvertiser
    {
        /// <summary> Binds the advertiser to its host entity. </summary>
        /// <param name="host"> The entity at whose location the rest action will be offered. </param>
        public RestSpotAdvertiser(Entity host) : base(host) {}


        /// <inheritdoc/>
        public override IEnumerable<ActorAction> GetAdvertisedActions(ActorController querier)
        {
            List<ActorAction> actions = new List<ActorAction>();

            ActorAction restAction = new ActorAction.Builder($"Rest_{_host.Name}", new GoToEntityStrategy(querier.Actor, _host))
                .WithDistanceCost(querier.Actor, _host)
                .AddOutcome(querier.AvailableFacts["is_fresh"])
                .Build();

            actions.Add(restAction);

            return actions;
        }
    }
}
