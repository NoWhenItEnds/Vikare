using Godot;
using Godot.Collections;

namespace Vikare.Entities.Combat
{
    /// <summary>
    /// Describes a single castable power — its animation, timing, optional chain targets, and the effect that fires mid-cast.
    /// Create a <c>.tres</c> asset per power (e.g. <c>Teleport.tres</c>, <c>Fireball.tres</c>);
    /// wire chains by populating <see cref="NextPowers"/>. No code changes are needed to add new powers.
    /// </summary>
    [GlobalClass]
    public partial class PowerDefinition : Resource
    {
        /// <summary>
        /// Name of the animation clip to play when casting this power; must match a clip in the entity's animation library.
        /// </summary>
        [Export] public string AnimationName { get; set; } = string.Empty;

        /// <summary>
        /// Total cast duration in seconds; the casting state transitions to idle once this elapses with no chain input accepted.
        /// The effective duration is rounded up to the next physics tick.
        /// </summary>
        [Export] public double DurationSeconds { get; set; } = 0.6;

        /// <summary>
        /// Time offset in seconds from cast start at which <see cref="Effect"/> fires.
        /// Must be less than <see cref="DurationSeconds"/>; fired at most once per cast activation.
        /// </summary>
        [Export] public double EffectFrameSeconds { get; set; } = 0.3;

        /// <summary>
        /// Earliest time in seconds at which a chain <see cref="Vikare.Entities.Controllers.CastPowerIntent"/> is accepted.
        /// </summary>
        [Export] public double CancelWindowStartSeconds { get; set; } = 0.4;

        /// <summary>
        /// Latest time in seconds at which a chain <see cref="Vikare.Entities.Controllers.CastPowerIntent"/> is accepted.
        /// Input after this offset is ignored; the cast runs to <see cref="DurationSeconds"/> and returns to idle.
        /// </summary>
        [Export] public double CancelWindowEndSeconds { get; set; } = 0.55;

        /// <summary>
        /// Powers that can be chained to from this cast if a <see cref="Vikare.Entities.Controllers.CastPowerIntent"/>
        /// arrives in the cancel window and the intent's <c>Power</c> is present in this array.
        /// An empty array means this power is non-comboable (the degenerate case used by single-use powers like teleport).
        /// </summary>
        [Export] public Array<PowerDefinition> NextPowers { get; set; } = new();

        /// <summary>
        /// The effect resource to execute at <see cref="EffectFrameSeconds"/>; null is valid — a null effect means
        /// the cast plays its animation and transitions without producing any game-world change.
        /// Assign a concrete <see cref="PowerEffect"/> subclass asset (e.g. <c>TeleportPowerEffect.tres</c>) in the editor.
        /// </summary>
        [Export] public PowerEffect? Effect { get; set; }
    }
}
