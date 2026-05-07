using System.Collections.Generic;

namespace Vikare.Entities.GOAP.Advertisers
{
    /// <summary> An object in the world that offers actions to actors who can perceive it. </summary>
    public abstract class ActionAdvertiser
    {
        /// <summary> The entity offering the action. This entity. </summary>
        protected readonly Entity _host;


        /// <summary> An object in the world that offers actions to actors who can perceive it. </summary>
        /// <param name="host"> The entity offering the action. This entity. </param>
        public ActionAdvertiser(Entity host)
        {
            _host = host;
        }

        /// <summary> The actions this object offers to the querying actor. </summary>
        /// <param name="querier"> The controller of the actor that is asking what actions are on offer. </param>
        /// <returns> Zero or more actions whose preconditions and outcomes use facts from the controller's existing <c>AvailableFacts</c> vocabulary. </returns>
        public abstract IEnumerable<ActorAction> GetAdvertisedActions(ActorController querier);
    }
}
