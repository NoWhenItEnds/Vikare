using Godot;
using System;
using Vikare.Entities;
using Vikare.Entities.Components;
using Vikare.Utilities.Singletons;
using Logger = Vikare.Utilities.Logging.Logger;

namespace Vikare.Managers
{
    /// <summary> The game world's central manager singleton. </summary>
    public partial class GameManager : SingletonNode<GameManager>
    {
        [Export] private Actor _player = null!;

        /// <inheritdoc/>
        public override void _Ready()
        {
            Logger.Instance.Info("Hello, World!", Name);

            _player.TryAddComponent<AttributeComponent>();
            InputManager.Instance.RegisterPlayer(_player);
        }
    }
}
