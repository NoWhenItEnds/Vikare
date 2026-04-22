using Vikare.Entities.Combat;

namespace Vikare.Entities.Interfaces
{
    /// <summary>
    /// Capability contract for entities that can perform melee attack combos.
    /// Lives on <see cref="Vikare.Entities.Actor"/> rather than <see cref="Vikare.Entities.Entity"/>
    /// because attack behaviour requires animation playback (<see cref="IAnimated"/>), and non-actor
    /// entities (projectiles, static props, interactive objects) have no concept of an attack moveset.
    /// Restricting the interface to <c>Actor</c> prevents designers from accidentally assigning an
    /// <see cref="AttackSequence"/> to a node that can never use it.
    /// </summary>
    public interface IAttacker
    {
        /// <summary>
        /// The combo graph equipped by this entity; null when no weapon or moveset has been assigned.
        /// States null-guard via <c>?.</c> — missing sequence data silently aborts the attack.
        /// Designers wire this via the <c>[Export]</c> property on <see cref="Vikare.Entities.Actor"/>.
        /// </summary>
        AttackSequence? AttackSequence { get; }

        /// <summary>
        /// Called by <see cref="Vikare.Entities.States.AttackingState"/> at the <see cref="AttackStep.HitFrameSeconds"/>
        /// offset within each step. The base implementation is a no-op virtual hook; attach damage logic
        /// by overriding in a concrete actor subclass or via a separate system that listens for the hit frame
        /// to fire (e.g. an area query or a signal). Not declared on the state itself because the state
        /// is data-driven and should not own game logic beyond timing.
        /// </summary>
        /// <param name="step">The step whose hit frame just elapsed; use to access damage data once those fields are added to <see cref="AttackStep"/>.</param>
        void OnHitFrame(AttackStep step);
    }
}
