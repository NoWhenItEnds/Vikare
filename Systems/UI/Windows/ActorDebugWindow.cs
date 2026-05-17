using Godot;
using System;
using System.Text;
using Vikare.Entities;
using Vikare.Entities.Components;
using Vikare.Entities.GOAP;
using Vikare.Managers;

namespace Vikare.UI.Windows
{
    /// <summary> A debug window used to display an actor's stats and state. </summary>
    public partial class ActorDebugWindow : Control
    {
        [ExportGroup("Nodes")]
        [ExportSubgroup("Basic")]
        [Export] private RichTextLabel _needsLabel = null!;

        [Export] private RichTextLabel _attributesLabel = null!;

        [ExportSubgroup("GOAP")]
        [Export] private RichTextLabel _factsLabel = null!;


        /// <summary> The actor associated with this window. A null indicates that there isn't one. </summary>
        private Actor? _actor = null;


        public void RegisterActor(Actor actor)
        {
            if (_actor != null)
            {
                DeregisterActor();
            }

            _actor = actor;
        }


        public void DeregisterActor()
        {
            _actor = null;
        }


        /// <inheritdoc/>
        public override void _PhysicsProcess(double delta)
        {
            // Set visibility.
            Visible = _actor != null;

            if (_actor != null)
            {
                // Update position.
                GlobalPosition = _actor.GetGlobalTransformWithCanvas().Origin;

                // Update needs.
                StringBuilder needsBuilder = new StringBuilder("[b]Needs[/b]\n");
                NeedsComponent? needs = _actor.GetComponent<NeedsComponent>();
                if(needs != null)
                {
                    needsBuilder.AppendLine($"Stamina - {needs.Stamina.CurrentValue:F1} / {needs.Stamina.MaxValue:F1} ({needs.Stamina.Percent:F2})");
                }
                _needsLabel.Text = needsBuilder.ToString();

                // Update facts.
                if(ActorManager.Instance.TryGetController(_actor, out ActorController? controller) && controller != null)
                {
                    //StringBuilder needsBuilder = new StringBuilder("[b]Needs[/b]\n");
                }
            }
        }
    }
}
