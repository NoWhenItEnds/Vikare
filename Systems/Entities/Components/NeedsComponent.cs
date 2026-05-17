using System;
using Godot;
using Vikare.Managers;
using Vikare.Types;

namespace Vikare.Entities.Components
{
    /// <summary> A component showing an entity's needs; their wants and desires. </summary>
    public partial class NeedsComponent : EntityComponent
    {
        public DerivedStat Stamina { get; set; }

        public DerivedStat Entertainment { get; set; }

        public DerivedStat Hydration { get; set; }


        private AttributeComponent? _attributeComponent = null;

        private DateTime? _lastTimePolled = null;


        public NeedsComponent()
        {
            Stamina = new DerivedStat(() => 0, CalculateMaximumStamina);
            Entertainment = new DerivedStat(() => 0, CalculateMaximumEntertainment);
            Hydration = new DerivedStat(() => 0, CalculateMaximumHydration);
        }


        public override void PhysicsProcess(Double delta)
        {
            DateTime currentTime = GameManager.Instance.CurrentTime;
            if (_lastTimePolled != null)
            {
                TimeSpan difference = (TimeSpan)(currentTime - _lastTimePolled);
                Double elapsedSeconds = difference.TotalSeconds * delta;
                Stamina.CurrentValue -= (Single)(0.1 * elapsedSeconds);
            }

            _lastTimePolled = currentTime;
        }


        /// <inheritdoc/>
        protected internal override void OnAddedTo(Entity entity)
        {
            _attributeComponent = entity.GetComponent<AttributeComponent>();
        }


        /// <inheritdoc/>
        protected internal override void OnRemovedFrom(Entity entity)
        {
            _attributeComponent = null;
        }


        private Single CalculateMaximumStamina()
        {
            Single result = 10f;

            if(_attributeComponent != null)
            {
                result = (3 + _attributeComponent.Constitution.CurrentValue) * 10f;
            }

            return result;
        }


        private Single CalculateMaximumEntertainment()
        {
            Single result = 10f;

            if (_attributeComponent != null)
            {
                result = (3 + _attributeComponent.Constitution.CurrentValue) * 10f;
            }

            return result;
        }


        private Single CalculateMaximumHydration()
        {
            Single result = 10f;

            if (_attributeComponent != null)
            {
                result = (3 + _attributeComponent.Constitution.CurrentValue) * 10f;
            }

            return result;
        }
    }
}
