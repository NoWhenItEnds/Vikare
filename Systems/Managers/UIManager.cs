using Godot;
using System;
using System.Collections.Generic;
using Vikare.Entities;
using Vikare.Entities.Components;
using Vikare.Utilities.Singletons;

namespace Vikare.Managers
{
    /// <summary> The main UI manager for the game world. </summary>
    public partial class UIManager : SingletonControl<UIManager>
    {
        [ExportGroup("Nodes")]
        [Export] private RichTextLabel _timeLabel = null!;
        [Export] private RichTextLabel _memoryLabel = null!;


        /// <inheritdoc/>
        public override void _PhysicsProcess(Double delta)
        {
            _timeLabel.Text = GameManager.Instance.CurrentTime.ToString("HH:mm");

            _memoryLabel.Text = "";
            Actor? player = ActorManager.Instance.Player;
            if(player != null)
            {
                MemoryComponent? memory = player.GetComponent<MemoryComponent>();
                if(memory != null)
                {
                    foreach (Entity entity in memory.GetEntities())
                    {
                        _memoryLabel.Text += $"{entity.Name}\n";
                    }
                }
            }
        }
    }
}
