using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Vikare.Entities.Abilities;
using Vikare.Entities.States;

namespace Vikare.Entities
{
    /// <summary> An entity that is controlled by something, whether that is the player or an AI controller. </summary>
    public partial class Actor : Entity
    {
        /// <summary>State machine governing this actor's behaviour.</summary>
        [ExportGroup("Nodes")]
        [Export] public StateMachine Machine { get; private set; } = null!;

        /// <summary>
        /// Ability graph used for melee attacks, power casts, and combo chains. Assign a <c>.tres</c>
        /// asset in the editor. Null means no moveset is equipped; <see cref="AbilityState"/> returns
        /// to idle immediately when this is null.
        /// </summary>
        [ExportGroup("Combat")]
        [Export] public AbilitySequence? AbilitySequence { get; set; }  // TODO - ?

        /// <summary> The abilities currently possessed by the actor. </summary>
        [ExportGroup("Settings")]
        [Export] public Godot.Collections.Array<AbilityEffect> Abilities { get; private set; } = new();

        /// <summary>Maximum speed in units/second applied during a dodge burst.</summary>
        [Export] public Single MaxDodgeSpeed { get; set; } = 8f;

        /// <summary>Duration in seconds of a single dodge burst before transitioning back to idle.</summary>
        [Export] public Double MaxDodgeDurationSeconds { get; set; } = 0.25;


        /// <summary>
        /// Returns the Nth ability in <see cref="Abilities"/> that matches <paramref name="category"/>,
        /// where N is zero-based and determined by order of appearance. Returns null when no match exists.
        /// </summary>
        /// <param name="category">The category to search for.</param>
        /// <param name="index">Zero-based position among matching abilities; defaults to the first match.</param>
        public AbilityEffect? GetAbility(AbilityCategory category, int index = 0)
        {
            AbilityEffect? result = GetAbilities(category)
                .ElementAtOrDefault(index);

            return result;
        }

        /// <summary>
        /// Returns all abilities in <see cref="Abilities"/> that match <paramref name="category"/>,
        /// in array order. Null slots (possible from partial editor assignment) are skipped.
        /// Uses lazy enumeration to avoid allocating a Godot collection.
        /// </summary>
        /// <param name="category">The category to filter by.</param>
        public IEnumerable<AbilityEffect> GetAbilities(AbilityCategory category)
        {
            return Abilities.Where(a => a != null && a.Category == category);
        }


        /// <inheritdoc/>
        public override void _PhysicsProcess(Double delta)
        {
            base._PhysicsProcess(delta);
            MoveAndSlide(); // End each frame with a slide.
        }
    }
}
