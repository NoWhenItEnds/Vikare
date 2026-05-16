using System;
using System.Collections.Generic;
using Godot;

namespace Vikare.Entities.GOAP.Advertisers
{
    /// <summary> An object in the world that offers facts and actions to actors who can perceive it. </summary>
    public abstract partial class ActionAdvertiser : Resource
    {
        /// <summary> Radius in world units within which an actor is considered adjacent to this advertiser. </summary>
        [ExportGroup("Settings")]
        [Export(PropertyHint.Range, "0.0,32.0,0.1,or_greater")]
        public Single InteractionRadius { get; set; } = 8f;

        /// <summary> The entity in the world that is hosting this advertiser. </summary>
        protected Entity _host = null!;

        /// <summary> Cached unique instance ID of the host entity, used to disambiguate facts and actions across same-named hosts. </summary>
        protected UInt64 _hostId;

        /// <summary> Guards against the host binding being set more than once after deserialisation. </summary>
        private Boolean _isInitialised = false;


        /// <summary> Binds the advertiser to the entity that will host it at runtime. </summary>
        /// <param name="host"> The entity at whose location the advertised actions will be performed. </param>
        public void Initialise(Entity host)
        {
            if (!_isInitialised)
            {
                _host = host;
                _hostId = host.GetInstanceId();
                _isInitialised = true;
            }
        }


        /// <summary> Returns the facts this advertiser contributes to the querying actor's world-model for one planning round. </summary>
        /// <param name="actor"> The actor requesting facts. </param>
        /// <returns> A dictionary of facts keyed by fact name, ready to be merged into <c>AvailableFacts</c>. </returns>
        /// <remarks> Each concrete advertiser is responsible for owning and providing the fact that its action targets; there is no shared baseline. </remarks>
        public abstract Dictionary<String, ActorFact> GetFacts(Actor actor);


        /// <summary> The actions this object offers to the querying actor. </summary>
        /// <param name="actor"> The actor requesting actions. </param>
        /// <param name="existingFacts"> The facts these actions will be constructed from. </param>
        /// <returns> Zero or more actions whose preconditions and outcomes use facts from the querier's <c>AvailableFacts</c> vocabulary. </returns>
        public abstract IEnumerable<ActorAction> GetActions(Actor actor, Dictionary<String, ActorFact> existingFacts);
    }
}
