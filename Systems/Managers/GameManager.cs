using Godot;
using System;
using Microsoft.Extensions.Logging;
using Vikare.Entities;
using Vikare.Entities.Components;
using Vikare.Utilities.Logging;
using Vikare.Utilities.Singletons;

namespace Vikare.Managers
{
    /// <summary> The game world's central manager singleton. </summary>
    public partial class GameManager : SingletonNode<GameManager>
    {
        /// <summary> Logger for this manager's operational messages. </summary>
        private static readonly ILogger Logger = Log.For<GameManager>();

        [Export] private Actor _player = null!;

        [Export] private Single _timescale = 12f;

        /// <summary> The current time within the game world. </summary>
        public DateTime CurrentTime { get; private set; } = DateTime.UtcNow;

        /// <inheritdoc/>
        public override void _Ready()
        {
            Logger.LogInformation("[{Node}] Hello, World!", Name);

            _player.TryAddComponent<AttributeComponent>();
            _player.TryAddComponent<NeedsComponent>();
            _player.TryAddComponent<MemoryComponent>();
            InputManager.Instance.RegisterPlayer(_player);
        }


        /// <inheritdoc/>
        public override void _PhysicsProcess(Double delta)
        {
            CurrentTime = CurrentTime.Add(TimeSpan.FromSeconds(delta * _timescale));
        }
    }
}
