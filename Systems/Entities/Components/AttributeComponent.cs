using Godot;
using Vikare.Types;

namespace Vikare.Entities.Components
{
    /// <summary> The attributes that define the basic capabilities of an entity. </summary>
    public partial class AttributeComponent : EntityComponent
    {
        [Export] public Stat Strength { get; set; } = new Stat("strength", 2, 0, 5);

        [Export] public Stat Finesse { get; set; } = new Stat("finesse", 2, 0, 5);

        [Export] public Stat Constitution { get; set; } = new Stat("Constitution", 2, 0, 5);

        [Export] public Stat Intelligence { get; set; } = new Stat("intelligence", 2, 0, 5);

        [Export] public Stat Wits { get; set; } = new Stat("wits", 2, 0, 5);

        [Export] public Stat Presence { get; set; } = new Stat("presence", 2, 0, 5);
    }
}
