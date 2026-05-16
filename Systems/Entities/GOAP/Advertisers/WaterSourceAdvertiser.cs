using System;
using System.Collections.Generic;
using Godot;
using Vikare.Entities.Components;
using Vikare.Entities.GOAP.Strategies;

namespace Vikare.Entities.GOAP.Advertisers
{
    /// <summary> An object an actor can interact with to restore hydration — wells, rivers, troughs, etc. </summary>
    [GlobalClass]
    public partial class WaterSourceAdvertiser : ActionAdvertiser
    {
        /// <summary>
        /// Hydration restored per second whilst the actor is drinking from this source.
        /// With the default <c>DerivedStat</c> range of [0, 10], a value of 5 fills the bar in two seconds.
        /// </summary>
        [ExportGroup("Settings")]
        [Export(PropertyHint.Range, "0.0,100.0,0.1,or_greater")]
        public Single HydrationRate { get; set; } = 5f;


        /// <inheritdoc/>
        public override Dictionary<String, ActorFact> GetFacts(Actor actor)
        {
            Dictionary<String, ActorFact> facts = new Dictionary<String, ActorFact>();

            String factKey = $"at_{_hostId}";
            Single atRangeSquared = InteractionRadius * InteractionRadius;

            facts.Add(factKey, new ActorFact.Builder(factKey)
                .WithCondition(() => actor.GlobalPosition.DistanceSquaredTo(_host.GlobalPosition) <= atRangeSquared)
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

            NeedsComponent? needs = actor.GetComponent<NeedsComponent>();

            if (needs != null && existingFacts.TryGetValue("is_hydrated", out ActorFact? isHydratedFact))
            {
                Single rate = HydrationRate;

                ActorAction useAction = new ActorAction.Builder(
                        $"Use_{_hostId}",
                        new UseAdvertiserStrategy(
                            actor,
                            _host,
                            duration: 1f,
                            interactionRadius: InteractionRadius,
                            onTick: delta => { needs.Hydration.CurrentValue += rate * delta; },
                            onComplete: () => { }))
                    .AddPrecondition(atFact)
                    .AddOutcome(isHydratedFact)
                    .Build();

                actions.Add(useAction);
            }

            return actions;
        }
    }
}
