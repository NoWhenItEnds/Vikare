using System;
using Godot;

namespace Vikare.Entities.Abilities
{
    /// <summary> A component that provides an entity with the information needed to move. </summary>
    [GlobalClass]
    public partial class MovementComponent : Resource
    {
        /// <summary> Maximum walk speed in pixels per second. </summary>
        [Export] public Single MaxSpeed { get; set; } = 200f;

        /// <summary> How much to modify <see cref="MaxSpeed"/> by when running. </summary>
        [Export] public Single SprintModifier{ get; set; } = 2f;

        /// <summary> How much to modify <see cref="MaxSpeed"/> by when dodging. </summary>
        [Export] public Single DodgeModifier { get; set; } = 600f;

        /// <summary> Duration of a single dodge burst in seconds. </summary>
        [Export] public Single DodgeDurationSeconds { get; set; } = 0.25f;
    }
}
