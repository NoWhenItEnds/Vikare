using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Vikare.Types
{
    /// <summary> A class representing a generic statistic. </summary>
    [GlobalClass]
    public partial class Stat : Resource, IEquatable<Stat>
    {
        /// <summary> The unique name or identifier for the statistic. </summary>
        [Export] public String Name { get; private set; }

        /// <summary> The statistic's minimum value. The actual value will be clamped to this. </summary>
        private Single _minValue;

        /// <summary> The statistic's minimum value. The actual value will be clamped to this. </summary>
        [Export] public Single MinValue
        {
            get
            {
                return _minValue;
            }
            set
            {
                _minValue = value;
                if (BaseValue < _minValue)
                {
                    BaseValue = _minValue;
                }
            }
        }

        /// <summary> The statistic's maximum value. The actual value will be clamped to this. </summary>
        private Single _maxValue;

        /// <summary> The statistic's maximum value. The actual value will be clamped to this. </summary>
        [Export] public Single MaxValue
        {
            get
            {
                return _maxValue;
            }
            set
            {
                _maxValue = value;
                if (BaseValue > _maxValue)
                {
                    BaseValue = _maxValue;
                }
            }
        }

        /// <summary> The statistic's base value before modifiers are applied. </summary>
        private Single _baseValue;

        /// <summary> The statistic's base value before modifiers are applied. </summary>
        [Export] public Single BaseValue
        {
            get
            {
                return _baseValue;
            }
            set
            {
                _baseValue = Math.Clamp(value, MinValue, MaxValue);
                CalculateValue();
            }
        }

        /// <summary> When the statistic changes, this is its previous value. </summary>
        public Single PreviousValue { get; private set; }

        /// <summary> The current value of the statistic after modifiers have been applied. </summary>
        public Single CurrentValue { get; private set; }

        /// <summary> Emitted when the value is changed. Contains the new current value for the statistic. </summary>
        public event Action<Single> ValueChanged;

        /// <summary> An amount, from 0.0 - 1.0 the statistic is between its min and max value. </summary>
        public Single Percent => (CurrentValue - MinValue) / (MaxValue - MinValue);


        /// <summary> The modifiers currently being applied to the stat. </summary>
        private HashSet<StatModifier> _modifiers = new HashSet<StatModifier>();


        /// <summary> A class representing a generic statistic. </summary>
        public Stat() { }


        /// <summary> A class representing a generic statistic. </summary>
        /// <param name="name"> The unique name or identifier for the statistic. </param>
        /// <param name="baseValue"> The statistic's base value before modifiers are applied. </param>
        /// <param name="minValue"> The statistic's minimum value. The actual value will be clamped to this. </param>
        /// <param name="maxValue"> The statistic's maximum value. The actual value will be clamped to this. </param>
        public Stat(String name, Single baseValue, Single minValue, Single maxValue)
        {
            Name = name;
            MinValue = minValue;
            MaxValue = maxValue;
            BaseValue = baseValue;
        }


        /// <summary> Adds a modifier to the stat. </summary>
        /// <param name="modifier"> The modifier object to add. </param>
        public void AddModifier(StatModifier modifier)
        {
            if (_modifiers.Add(modifier))
            {
                CalculateValue();
            }
        }


        /// <summary> Adds an array of modifiers to the stat. </summary>
        /// <param name="modifiers"> The modifier array to add. </param>
        public void AddModifier(StatModifier[] modifiers)
        {
            Single added = 0;
            foreach (StatModifier modifier in modifiers)
            {
                added += _modifiers.Add(modifier) ? 1 : 0;
            }

            if (added > 0)
            {
                CalculateValue();
            }
        }


        /// <summary> Removes a modifier from the stat. </summary>
        /// <param name="modifier"> The modifier object to remove. </param>
        public void RemoveModifier(StatModifier modifier)
        {
            if (_modifiers.Remove(modifier))
            {
                CalculateValue();
            }
        }


        /// <summary> Removes an array of modifiers from the stat. </summary>
        /// <param name="modifiers"> The modifier array to remove. </param>
        public void RemoveModifier(StatModifier[] modifiers)
        {
            Single removed = 0;
            foreach (StatModifier modifier in modifiers)
            {
                removed += _modifiers.Remove(modifier) ? 1 : 0;
            }

            if (removed > 0)
            {
                CalculateValue();
            }
        }


        /// <summary> Remove all the modifiers added from a particular source. </summary>
        /// <param name="source"> A reference to the source object. </param>
        public void RemoveModifierSource(Object source)
        {
            StatModifier[] filtered = _modifiers.Where(x => x.Source == source).ToArray();
            if (filtered.Length > 0)
            {
                RemoveModifier(filtered);
            }
        }


        /// <summary> Remove all the active modifiers. </summary>
        public void ClearModifiers()
        {
            if (_modifiers.Count > 0)
            {
                _modifiers.Clear();
                CalculateValue();
            }
        }


        /// <summary> Calculate the stat's current value. </summary>
        private void CalculateValue()
        {
            Single oldCurrent = CurrentValue;
            CurrentValue = BaseValue;

            foreach (StatModifier modifier in _modifiers.OrderBy(x => x.Priority))
            {
                CurrentValue = modifier.Apply(CurrentValue);
            }

            if (oldCurrent != CurrentValue)
            {
                PreviousValue = oldCurrent;
                ValueChanged?.Invoke(CurrentValue);
            }
        }


        /// <inheritdoc/>
        public override Int32 GetHashCode() => HashCode.Combine(Name);


        /// <inheritdoc/>
        public Boolean Equals(Stat? other) => other != null && Name == other.Name;
    }


    /// <summary> A modifier that can be applied to a generic stat. </summary>
    public record StatModifier
    {
        /// <summary> How the modifier affects the value. </summary>
        public enum StatOperation
        {
            ADD,
            SUBTRACT,
            MULTIPLY,
            DIVIDE
        }

        /// <summary> The order the modifier should be applied to the stat. Lower is applied first. </summary>
        public Single Priority { get; init; } = 0;

        /// <summary> What object is applying the modification. </summary>
        /// <remarks> A null indicates that there isn't one. </remarks>
        public Object? Source { get; init; } = null;

        /// <summary> How the modifier affects the value. </summary>
        public required StatOperation Operation { get; init; }

        /// <summary> The actual value that affects the entity stat. </summary>
        public required Single Value { get; init; }


        /// <summary> Functionally apply the modifier to the given current value. </summary>
        /// <param name="currentValue"> The current value to modify. </param>
        /// <returns> The result of the operation. </returns>
        public Single Apply(Single currentValue)
        {
            Single result = currentValue;

            switch (Operation)
            {
                case StatOperation.ADD:
                    result = currentValue + Value;
                    break;
                case StatOperation.SUBTRACT:
                    result = currentValue - Value;
                    break;
                case StatOperation.MULTIPLY:
                    result = currentValue * Value;
                    break;
                case StatOperation.DIVIDE:
                    result = currentValue / Value;
                    break;
                default:
                    break;
            }

            return result;
        }
    }
}
