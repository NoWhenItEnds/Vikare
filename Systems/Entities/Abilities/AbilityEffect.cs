using Godot;

namespace Vikare.Entities.Abilities
{
    /// <summary> Abstract base resource for any effect that fires at a designated frame within an ability step. </remarks>
    [GlobalClass]
    public abstract partial class AbilityEffect : Resource
    {
        /// <summary> Slot identity. Each concrete subclass overrides this with its fixed category. </summary>
        public abstract AbilityCategory Category { get; }

        /// <summary> Executes this effect's game logic at the effect frame within the ability step. </summary>
        /// <param name="actor"> The actor executing the ability step. </param>
        public abstract void Execute(Actor actor);
    }
}
