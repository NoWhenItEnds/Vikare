using Godot;
using Vikare.Entities.Interfaces;

namespace Vikare.Entities.Combat
{
    /// <summary>
    /// Abstract base resource for the effect that fires at <see cref="PowerDefinition.EffectFrameSeconds"/> during a cast.
    /// Subclass this resource (e.g. <see cref="TeleportPowerEffect"/>, <see cref="FireballPowerEffect"/>) and override
    /// <see cref="Execute"/> to implement the actual game logic. Create a <c>.tres</c> asset of the concrete subclass
    /// and assign it to <see cref="PowerDefinition.Effect"/> — no code changes required to add new effects.
    ///
    /// Design note: Godot cannot serialise an interface via <c>[Export]</c>; only <see cref="Resource"/> subclasses can be
    /// assigned in the editor. Using an abstract resource base class is therefore the idiomatic OCP extension point here,
    /// at the cost of requiring all effects to inherit from <see cref="Resource"/>. This is an acceptable trade-off for a
    /// Godot data-driven workflow — the alternative (a hand-rolled factory or reflection-based registry) adds complexity
    /// for no practical gain on a single-team project.
    /// </summary>
    [GlobalClass]
    public abstract partial class PowerEffect : Resource
    {
        /// <summary>
        /// Executes the effect's game logic at the designated frame within the cast.
        /// Implementations must null-guard any capability they request from <paramref name="context"/>
        /// because the effect may be executed on an entity that does not support all capabilities.
        /// </summary>
        /// <param name="context">Entity context at the moment the effect fires; use <c>As&lt;T&gt;()</c> to query capabilities.</param>
        public abstract void Execute(IStateContext context);
    }
}
