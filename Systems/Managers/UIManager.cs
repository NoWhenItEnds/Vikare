using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Vikare.Entities;
using Vikare.UI.Windows;
using Vikare.Utilities.Singletons;

namespace Vikare.Managers
{
    /// <summary> The main UI manager for the game world. </summary>
    public partial class UIManager : SingletonControl<UIManager>
    {
        [ExportGroup("Nodes")]
        [Export] private RichTextLabel _timeLabel = null!;
        [Export] private RichTextLabel _memoryLabel = null!;


        [ExportGroup("Resources")]
        [Export] private PackedScene _actorDebugWindowPrefab = null!;


        private readonly Dictionary<ActorDebugWindow, Actor?> _debugWindowMap = new Dictionary<ActorDebugWindow, Actor?>();


        /// <inheritdoc/>
        public override void _Ready()
        {
            for (Int32 i = 0; i < 100; i++)
            {
                ActorDebugWindow? window = _actorDebugWindowPrefab.InstantiateOrNull<ActorDebugWindow>();
                if(window != null)
                {
                    AddChild(window);
                    _debugWindowMap.Add(window, null);
                }
            }
        }


        /// <inheritdoc/>
        public override void _PhysicsProcess(Double delta)
        {
            _timeLabel.Text = GameManager.Instance.CurrentTime.ToString("HH:mm");


            Dictionary<ActorDebugWindow, Actor?> dirtyWindows = new Dictionary<ActorDebugWindow, Actor?>(_debugWindowMap.Where(x => x.Value != null));   // All the windows with an associated actor.
            Dictionary<ActorDebugWindow, Actor?> cleanWindows = new Dictionary<ActorDebugWindow, Actor?>(_debugWindowMap.Where(x => x.Value == null));
            foreach (Actor actor in GetVisibleActors())
            {
                // Check if there is already a window associated.
                Boolean hasWindow = false;
                foreach (KeyValuePair<ActorDebugWindow, Actor?> window in dirtyWindows)
                {
                    if (window.Value != null && window.Value.Equals(actor))
                    {
                        dirtyWindows.Remove(window.Key);
                        hasWindow = true;
                        break;
                    }
                }

                // If there's not already a window associated with the entity.
                if (!hasWindow)
                {
                    ActorDebugWindow window = cleanWindows.Keys.First();
                    cleanWindows.Remove(window);
                    _debugWindowMap[window] = actor;
                    window.RegisterActor(actor);
                }
            }

            // Perform final clean of dirty windows.
            foreach (KeyValuePair<ActorDebugWindow, Actor?> window in dirtyWindows)
            {
                window.Key.DeregisterActor();
                _debugWindowMap[window.Key] = null;
            }
        }


        private IEnumerable<Actor> GetVisibleActors()
        {
            List<Actor> actors = new List<Actor>();
            foreach (Actor actor in ActorManager.Instance.Actors)
            {
                Boolean isWithinCamera = GetViewport().GetVisibleRect().HasPoint(actor.GetGlobalTransformWithCanvas().Origin);
                if(isWithinCamera)
                {
                    actors.Add(actor);
                }
            }
            return actors;
        }
    }
}
