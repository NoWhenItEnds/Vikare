using Godot;
using Vikare.Types;

namespace Vikare.Entities.Components
{
    /// <summary> A component showing an entity's needs; their wants and desires. </summary>
    public partial class NeedsComponent : EntityComponent
    {
        public DerivedStat Stamina { get; set; } = new DerivedStat(() => 0, () => 10);

        public DerivedStat Entertainment { get; set; } = new DerivedStat(() => 0, () => 10);
    }
}
