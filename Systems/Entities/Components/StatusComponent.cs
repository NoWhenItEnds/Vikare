using System;
using Godot;
using Vikare.Types;

namespace Vikare.Entities.Components
{
    /// <summary> A component showing an entity's general status; its health and present state. </summary>
    public partial class StatusComponent : EntityComponent
    {
        public DerivedStat CurrentHealth { get; set; } = new DerivedStat(() => 0, () => 10);
    }
}
