using System;
using System.Collections.Generic;

namespace Vikare.Types
{
    /// <summary> All the positions / sockets that compose an entity. </summary>
    /// <remarks> These are the possible components an entity can possess. </remarks>
    [Flags]
    public enum EntityPart
    {
        None = 0,
        Head = 1,
        Hair = 2,
        Ears = 4,
        Eyes = 8,
        Face = 16,
        Torso = 32,
        Chest = 64,
        Wrists = 128,
        Hands = 256,
        Weapon = 512,
        Legs = 1024,
        Ankles = 2048,
        Feet = 4096,
        Genitals = 8192,
        Anus = 16384,
        Back = 32768
    }


    /// <summary> Helpful methods for working with EntityPart. </summary>
    public static class EntityPartExtensions
    {
        /// <summary> Deconstruct a bitwise operator back into an array of enumerable enums. </summary>
        /// <param name="value"> The combined bitwise value. </param>
        /// <returns> An array of enums present within the input. </returns>
        public static EntityPart[] ToEnums(this Int32 value)
        {
            List<EntityPart> results = new List<EntityPart>();

            EntityPart parts = (EntityPart)value;
            foreach (EntityPart part in Enum.GetValues(typeof(EntityPart)))
            {
                // Skip the "None" or zero-value flag.
                if (Convert.ToInt32(part) != 0 && parts.HasFlag(part))
                {
                    results.Add(part);
                }
            }

            return results.ToArray();
        }
    }
}
