using System;
using Godot;
using Vikare.Types;

namespace  Vikare.Utilities.Extensions
{
    /// <summary> Helpful methods for working with Vector2. </summary>
    public static class Vector2Extensions
    {
        /// <summary> Convert a raw vector into a 4-way direction. </summary>
        /// <param name="vector"> The vector to pull a direction from. </param>
        /// <returns> The vector's direction decimated to 4-ways. </returns>
        public static Direction ToDirection(this Vector2 vector)
        {
            Direction result = Direction.None;
            // Ignore tiny vectors to prevent jitter
            if (vector.LengthSquared() >= 0.1f) // Ignore tiny vectors to prevent jitter
            {
                // Normalise and compare magnitude of axes.
                if (Mathf.Abs(vector.X) > Mathf.Abs(vector.Y))
                {
                    // Horizontal is dominant
                    result = vector.X > 0 ? Direction.Right : Direction.Left;
                }
                else
                {
                    // Vertical is dominant
                    result = vector.Y > 0 ? Direction.Down : Direction.Up; // Y down is + in Godot
                }
            }

            return result;
        }


        /// <summary> Returns a random 2-D offset uniformly distributed within a disk. </summary>
        /// <returns> A new random position offset from the original position. </returns>
        /// <remarks> The square-root transform on the radius corrects the centre-bias that arises from sampling radius linearly. Without it positions cluster near the original's position. </remarks>
        public static Vector2 RandomOffset(this Vector2 position, Single spawnRadius)
        {
            Random random = Random.Shared;
            Single angle = (Single)(random.NextDouble() * Math.PI * 2.0);
            Single radius = (Single)(Math.Sqrt(random.NextDouble()) * spawnRadius);
            return position + new Vector2(MathF.Cos(angle) * radius, MathF.Sin(angle) * radius);
        }
    }
}
