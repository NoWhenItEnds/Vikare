using System;
using Godot;

namespace Vikare.Entities.Components
{
    /// <summary> The attributes that define the basic capabilities of an entity. </summary>
    public partial class AttributeComponent : EntityComponent
    {
        [Export] public Int32 Strength { get; set; } = 2;

        [Export] public Int32 Finesse { get; set; } = 2;

        [Export] public Int32 Constitution { get; set; } = 2;

        [Export] public Int32 Intelligence { get; set; } = 2;

        [Export] public Int32 Wits { get; set; } = 2;

        [Export] public Int32 Presence { get; set; } = 2;
    }
}
