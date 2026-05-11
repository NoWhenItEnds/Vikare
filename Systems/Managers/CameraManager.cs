using Godot;
using System;
using Vikare.Utilities.Singletons;

namespace Vikare.Managers
{
    /// <summary> The central manager for all cameras within the game world. </summary>
    public partial class CameraManager : SingletonNode2D<CameraManager>
    {
        /// <summary> The main camera the player uses to view the game world. </summary>
        [ExportGroup("Nodes")]
        [Export] private Camera2D _mainCamera = null!;


        /// <summary> A cached reference to the global input manager. </summary>
        private InputManager _inputManager = null!;


        /// <inheritdoc/>
        public override void _Ready()
        {
            _inputManager = InputManager.Instance;
        }


        /// <inheritdoc/>
        public override void _PhysicsProcess(Double delta)
        {
            _mainCamera.GlobalPosition = _inputManager.ViewPosition;
        }
    }
}
