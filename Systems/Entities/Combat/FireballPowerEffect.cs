using Godot;
using Vikare.Entities.Interfaces;

namespace Vikare.Entities.Combat
{
    /// <summary>
    /// A <see cref="PowerEffect"/> placeholder for projectile-based fire attacks.
    /// Projectile spawning requires a scene reference, pooling strategy, and a direction source — none of which
    /// are in scope for this iteration. This class exists so designers can assign a <c>FireballPowerEffect.tres</c>
    /// asset to a power today and have real behaviour appear here once those systems are built, without touching
    /// any other code. When projectile spawning is implemented, override <see cref="Execute"/> in this class only.
    /// </summary>
    [GlobalClass]
    public partial class FireballPowerEffect : PowerEffect
    {
        /// <summary>
        /// Stub implementation; logs a one-time message in debug builds and does nothing in release builds.
        /// Replace the body of this method when projectile spawning is implemented.
        /// No other files need to change — <see cref="PowerDefinition"/> and <see cref="Vikare.Entities.States.CastingPowerState"/>
        /// already call <c>Effect?.Execute(context)</c> polymorphically.
        /// </summary>
        /// <param name="context">Entity context; unused in this stub.</param>
        public override void Execute(IStateContext context)
        {
            // STUB: replace with projectile spawn logic when the projectile capability exists.
            GD.Print("[FireballPowerEffect] Projectile spawning is not yet implemented. Assign a PackedScene and spawn logic here.");
        }
    }
}
