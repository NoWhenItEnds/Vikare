using System;
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


        /// <inheritdoc/>
        public override void _Ready()
        {
            base._Ready();
        }


        /// <inheritdoc/>
        public override void _PhysicsProcess(Double delta)
        {
            base._PhysicsProcess(delta);
            MoveAndSlide(); // End each frame with a slide.
        }
    }
}
