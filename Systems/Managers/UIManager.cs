using Godot;
using System;
using Vikare.Utilities.Singletons;

namespace Vikare.Managers
{
    /// <summary> The main UI manager for the game world. </summary>
    public partial class UIManager : SingletonControl<UIManager>
    {
        [ExportGroup("Nodes")]
        [Export] private RichTextLabel _timeLabel = null!;


        /// <inheritdoc/>
        public override void _PhysicsProcess(Double delta)
        {
            _timeLabel.Text = GameManager.Instance.CurrentTime.ToString("HH:mm");
        }
    }
}
