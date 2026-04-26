using Godot;

namespace Vikare.Entities.Combat
{
    /// <summary>
    /// Abstract base resource for any effect that fires at a designated frame within an ability step.
    /// Subclass and override <see cref="Execute"/> to implement the game logic, then create a
    /// <c>.tres</c> asset and assign it to <see cref="AbilityStep.Effect"/>. No code changes are
    /// required to add new effect types (OCP).
    /// </summary>
    /// <remarks>
    /// Godot cannot serialise an interface via <c>[Export]</c>; using an abstract <see cref="Resource"/>
    /// base class is the idiomatic OCP extension point for editor-assignable polymorphic data.
    /// </remarks>
    [GlobalClass]
    public abstract partial class AbilityEffect : Resource
    {
        /// <summary>
        /// Executes this effect's game logic at the effect frame within the ability step.
        /// Called at most once per step activation by <see cref="Vikare.Entities.States.AbilityState"/>.
        /// </summary>
        /// <param name="actor">The actor executing the ability step.</param>
        public abstract void Execute(Actor actor);
    }
}
