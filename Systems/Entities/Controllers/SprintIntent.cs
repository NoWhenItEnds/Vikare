using System;
using Vikare.Entities.Interfaces;

namespace Vikare.Entities.Controllers
{
    /// <summary> An input expressing whether the controller wants the entity to sprint. </summary>
    public sealed class SprintIntent : IInputIntent
    {
        /// <summary> True when the sprint should begin; false when it should end. </summary>
        public Boolean IsSprinting { get; }


        /// <summary> An input expressing whether the controller wants the entity to sprint. </summary>
        /// <param name="isSprinting"> True when the sprint should begin; false when it should end. <</param>
        public SprintIntent(Boolean isSprinting)
        {
            IsSprinting = isSprinting;
        }
    }
}
