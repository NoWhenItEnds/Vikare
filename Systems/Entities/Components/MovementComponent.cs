using System;
using Godot;

namespace Vikare.Entities.Components
{
    /// <summary> A component that provides an entity with the information needed to move. </summary>
    public partial class MovementComponent : EntityComponent
    {
        /// <summary> Maximum walk speed in pixels per second. </summary>
        [Export] public Single MaxSpeed { get; set; } = 200f;

        /// <summary> How much to modify <see cref="MaxSpeed"/> by when running. </summary>
        [Export] public Single SprintModifier{ get; set; } = 2f;
    }
}
