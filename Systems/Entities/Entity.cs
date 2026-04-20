using Godot;

namespace Vikare.Entities
{
    /// <summary> An interactable entity within the game world. </summary>
    public partial class Entity : CharacterBody2D
    {
        /// <summary> The entity's collider. </summary>
        [ExportGroup("Nodes")]
        [Export] public CollisionShape2D Collision { get; private set; }
    }
}
