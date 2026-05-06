using System;
using Godot;

namespace Vikare.Utilities.Extensions
{
    /// <summary> Helpful methods for working with string names. </summary>
    public static class StringNameExtensions
    {
        /// <summary> Cast the string to lowercase characters. </summary>
        /// <param name="input"> The input string. </param>
        /// <returns> The input formatted to use lower case. </returns>
        /// <remarks> Uses LowerInvariant to address the Turkish-locale case-folding bug. </remarks>
        public static String ToLower(this StringName input) => ((String)input).ToLowerInvariant();


        /// <summary> Cast the string to uppercase characters. </summary>
        /// <param name="input"> The input string. </param>
        /// <returns> The input formatted to use upper case. </returns>
        /// <remarks> Uses UpperInvariant to address the Turkish-locale case-folding bug. </remarks>
        public static String ToUpper(this StringName input) => ((String)input).ToUpperInvariant();
    }
}
