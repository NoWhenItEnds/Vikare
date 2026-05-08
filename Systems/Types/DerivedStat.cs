using System;

namespace Vikare.Types
{
    /// <summary> A statistic whose values are based upon those of another. </summary>
    public class DerivedStat
    {
        /// <summary> The function used to calculate the current minimum possible value. </summary>
        private Func<Single> _minValue;

        /// <summary> The stat's minimum possible value. </summary>
        public Single MinValue
        {
            get
            {
                return _minValue();
            }
        }

        /// <summary> The function used to calculate the current maximum possible value. </summary>
        private Func<Single> _maxValue;

        /// <summary> The stat's maximum possible value. </summary>
        public Single MaxValue
        {
            get
            {
                return _maxValue();
            }
        }

        /// <summary> The current value of the stat. </summary>
        /// <remarks> This value can be outside the limits if the limits are changed so that the value is outside the bounds. This is intentional. </remarks>
        private Single _currentValue;

        /// <summary> The current value of the stat. </summary>
        public Single CurrentValue
        {
            get
            {
                return _currentValue;
            }
            set
            {
                _currentValue = Math.Clamp(value, MinValue, MaxValue);
                ValueChanged?.Invoke(_currentValue);
            }
        }

        /// <summary> Emitted when the value is changed. Contains the new current value for the statistic. </summary>
        public event Action<Single> ValueChanged;

        /// <summary> Normalised value in <c>[0, 1]</c> representing how far <c>CurrentValue</c> sits between <c>MinValue</c> and <c>MaxValue</c>. </summary>
        public Single Percent => (CurrentValue - MinValue) / (MaxValue - MinValue);


        /// <summary> A statistic whose values are based upon those of another. </summary>
        /// <param name="minValue"> The function used to calculate the current minimum possible value. </param>
        /// <param name="maxValue"> The function used to calculate the current maximum possible value. </param>
        public DerivedStat(Func<Single> minValue, Func<Single> maxValue)
        {
            _minValue = minValue;
            _maxValue = maxValue;
            CurrentValue = MaxValue;
        }


        /// <summary> A statistic whose values are based upon those of another. </summary>
        /// <param name="minValue"> The function used to calculate the current minimum possible value. </param>
        /// <param name="maxValue"> The function used to calculate the current maximum possible value. </param>
        /// <param name="initialValue"> The initial value to set the stat to. </param>
        public DerivedStat(Func<Single> minValue, Func<Single> maxValue, Single initialValue)
        {
            _minValue = minValue;
            _maxValue = maxValue;
            CurrentValue = initialValue;
        }
    }
}
