using System;
using System.Collections.Generic;
using Godot;
using Vikare.Entities;
using Vikare.Entities.Components;
using Vikare.Entities.GOAP;
using Vikare.Utilities.Extensions;
using Vikare.Utilities.Singletons;

namespace Vikare.Managers
{
    /// <summary> The game-world manager for all actors and their controllers. </summary>
    public partial class ActorManager : SingletonNode2D<ActorManager>
    {
        /// <summary> The actor's prefab. </summary>
        [ExportGroup("Resources")]
        [Export] private PackedScene _actorPrefab = null!;


        /// <summary> Total number of actors to spawn. </summary>
        [ExportGroup("Settings")]
        [Export] private Int32 _spawnCount = 3;

        /// <summary> Radius in pixels around this node's <c>GlobalPosition</c> within which actors are placed. </summary>
        [Export] private Single _spawnRadius = 200f;


        /// <summary> A map between actors and their linked controllers. </summary>
        public readonly Dictionary<Actor, ActorController?> _actors = new Dictionary<Actor, ActorController?>();


        /// <inheritdoc/>
        public override void _Ready()
        {
            for (Int32 i = 0; i < _spawnCount; i++)
            {
                Vector2 position = GlobalPosition.RandomOffset(_spawnRadius);
                Actor actor = SpawnActor(position);
                ActorController controller = new ActorController(actor, false);
                _actors.Add(actor, controller);
            }
        }


        /// <inheritdoc/>
        public override void _PhysicsProcess(Double delta)
        {
            foreach (ActorController? controller in _actors.Values)
            {
                if(controller != null)
                {
                    controller.ProcessPlan(delta);
                }
            }
        }


        /// <summary> Create a new actor entity. </summary>
        /// <param name="position"> The position to spawn the entity. </param>
        /// <returns> The newly created entity. </returns>
        /// <exception cref="ArgumentNullException"/>
        private Actor SpawnActor(Vector2 position)
        {
            Actor actor = _actorPrefab.InstantiateOrNull<Actor>() ?? throw new ArgumentNullException("Unable to convert the Actor prefab into its own class.");
            actor.GlobalPosition = position;
            AddChild(actor);

            AttributeComponent? attributes = actor.TryAddComponent<AttributeComponent>();
            NeedsComponent? needs = actor.TryAddComponent<NeedsComponent>();
            StatusComponent? status = actor.TryAddComponent<StatusComponent>();
            NameComponent name = NameComponent.Random();
            actor.TryAddComponent(name);
            MemoryComponent? memory = actor.TryAddComponent<MemoryComponent>();

            actor.Name = $"Actor_{name.ToString()}";

            needs?.Entertainment.CurrentValue = 1;  // TODO - Just for now.

            return actor;
        }
    }
}
