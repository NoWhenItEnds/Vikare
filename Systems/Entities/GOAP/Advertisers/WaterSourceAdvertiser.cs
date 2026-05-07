using System.Collections.Generic;
using Vikare.Entities.GOAP.Strategies;

namespace Vikare.Entities.GOAP.Advertisers
{
    /// <summary> Rest those weary bones, traveller. Hydrate yourself. Drink from a fountain everlasting. </summary>
    public class WaterSourceAdvertiser : ActionAdvertiser
    {
        /// <summary> Rest those weary bones, traveller. Hydrate yourself. Drink from a fountain everlasting. </summary>
        /// <param name="host"> The entity at whose location the drink action will be offered. </param>
        public WaterSourceAdvertiser(Entity host) : base(host) {}


        /// <inheritdoc/>
        public override IEnumerable<ActorAction> GetAdvertisedActions(ActorController querier)
        {
            List<ActorAction> actions = new List<ActorAction>();

            ActorAction drinkAction = new ActorAction.Builder($"Drink_{_host.Name}", new GoToEntityStrategy(querier.Actor, _host))
                .WithDistanceCost(querier.Actor, _host)
                .AddOutcome(querier.AvailableFacts["is_hydrated"])
                .Build();

            actions.Add(drinkAction);

            return actions;
        }
    }
}
