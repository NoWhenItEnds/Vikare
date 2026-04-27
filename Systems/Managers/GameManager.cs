using Godot;
using System;
using Vikare.Entities;
using Vikare.Utilities.Singletons;

namespace Vikare.Managers
{
    /// <summary> The game world's central manager singleton. </summary>
    public partial class GameManager : SingletonNode<GameManager>
    {
        [Export] private Actor _player;

        /// <inheritdoc/>
        public override void _Ready()
        {
            GD.Print("Hello, World!");
            InputManager.Instance.RegisterPlayer(_player);
        }
    }
}
