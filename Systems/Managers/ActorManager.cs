using System;
using System.Collections.Generic;
using Godot;
using Vikare.Entities;
using Vikare.Entities.Components;
using Vikare.Entities.GOAP;
using Vikare.Utilities.Singletons;

namespace Vikare.Managers
{
    /// <summary> The game-world manager for all actors and their controllers. </summary>
    public partial class ActorManager : SingletonNode2D<ActorManager>
    {
        /// <summary> The actor's prefab.</summary>
        [Export] private PackedScene _actorPrefab = null!;

        /// <summary> Total number of actors to spawn. </summary>
        [Export] private Int32 _spawnCount = 3;

        /// <summary> Radius in pixels around this node's <c>GlobalPosition</c> within which actors are placed. </summary>
        [Export] private Single _spawnRadius = 200f;

        /// <summary> Controllers for each successfully spawned actor. </summary>
        private readonly List<ActorController> _controllers = new List<ActorController>();

        /// <summary> Shared random instance; hoisted to avoid repeated allocation and the historical tight-loop seeding pitfall. </summary>
        private readonly Random _random = new Random();


        /// <inheritdoc/>
        public override void _Ready()
        {
            SpawnActors();
        }


        /// <inheritdoc/>
        public override void _PhysicsProcess(Double delta)
        {
            foreach (ActorController controller in _controllers)
            {
                controller.ProcessPlan(delta);
            }
        }


        /// <summary>Instantiates and configures all actors; invalid instantiations are skipped with an error log.</summary>
        private void SpawnActors()
        {
            for (Int32 i = 0; i < _spawnCount; i++)
            {
                Actor? actor = _actorPrefab.InstantiateOrNull<Actor>();
                if (actor != null)
                {
                    ConfigureActor(actor, i);
                }
            }
        }

        /// <summary> Constructs a new actor. </summary>
        /// <param name="actor"> The freshly instantiated actor to configure. </param>
        /// <param name="index"> Zero-based spawn index; drives position offset and need variation. </param>
        private void ConfigureActor(Actor actor, Int32 index)
        {
            actor.Position = RandomOffset();
            actor.Name = $"Actor_{index:000}";
            AddChild(actor);

            NeedsComponent needs = AddComponents(actor);
            ApplyNeedPattern(needs, index);

            ActorController controller = new ActorController(actor, true);
            _controllers.Add(controller);
        }

        /// <summary> Constructs the actor with the correct components. </summary>
        /// <param name="actor"> The actor whose component is retrieved or created. </param>
        /// <returns> The actor's <see cref="NeedsComponent"/>, guaranteed non-null. </returns>
        private NeedsComponent AddComponents(Actor actor)
        {
            NeedsComponent result;
            NeedsComponent? existing = actor.GetComponent<NeedsComponent>();

            if (existing != null)
            {
                result = existing;
            }
            else
            {
                NeedsComponent? added = actor.TryAddComponent<NeedsComponent>();
                result = added ?? actor.GetComponent<NeedsComponent>()!;
            }

            return result;
        }

        /// <summary>
        /// Sets the need values for a specific actor according to a three-pattern cycle so that distinct
        /// GOAP goals activate across the spawned population.
        ///
        /// Pattern (index mod 3):
        /// <list type="bullet">
        ///   <item>0 — Entertainment low (1), others full → selects <c>KeepEntertained</c>.</item>
        ///   <item>1 — Hydration low (1), others full → selects <c>StayHydrated</c>.</item>
        ///   <item>2 — All full → selects <c>WatchPaintDry</c> (no-op fallback).</item>
        /// </list>
        ///
        /// <c>DerivedStat</c> min = 0, max = 10; CurrentValue = 1 yields Percent = 0.1.
        /// The <c>KeepEntertained</c> and <c>StayHydrated</c> goals succeed only when
        /// <c>is_entertained</c> / <c>is_hydrated</c> evaluate true (Percent >= 0.9),
        /// so 0.1 keeps each goal active and selectable.
        /// </summary>
        /// <param name="needs">The component whose stat values are written.</param>
        /// <param name="index">Zero-based spawn index; only the modulo-3 remainder is used.</param>
        private void ApplyNeedPattern(NeedsComponent needs, Int32 index)
        {
            Int32 pattern = index % 3;

            if (pattern == 0)
            {
                // Entertainment critically low; Stamina and Hydration remain at max (10).
                needs.Entertainment.CurrentValue = 1;
            }
            else if (pattern == 1)
            {
                // Hydration critically low; Stamina and Entertainment remain at max (10).
                needs.Hydration.CurrentValue = 1;
            }
            // Pattern 2: all stats default to max — no action required.
        }

        /// <summary>
        /// Returns a random 2-D offset uniformly distributed within a disk of <see cref="SpawnRadius"/>.
        /// The square-root transform on the radius corrects the centre-bias that arises from sampling
        /// radius linearly — without it, actors cluster near the manager's position.
        /// </summary>
        /// <returns>A position vector relative to this node's origin.</returns>
        private Vector2 RandomOffset()
        {
            Single angle = (Single)(_random.NextDouble() * Math.PI * 2.0);
            Single radius = (Single)(Math.Sqrt(_random.NextDouble()) * _spawnRadius);
            return new Vector2(MathF.Cos(angle) * radius, MathF.Sin(angle) * radius);
        }
    }


    /*
        Open or create a test scene (e.g. res://Content/Scenes/GoapTest.tscn or wherever).
        Add a NavigationRegion2D, draw a navigation polygon, bake it. (Without baked navigation, actors selecting Wander will just stand still — the planner is still working, just WanderStrategy has no path.)
        Add a Node2D, attach Systems/Diagnostics/GoapTestManager.cs.
        Drag Content/Prefabs/Actor.tscn into the Actor Scene slot.
        Run. Watch the Output panel for traces like:

        Actor -> Calculating new plan...
        Actor -> Goal: KeepEntertained with 1 actions in plan.
        Actor -> Popped action: Wander.
        Actor -> Action, Wander, complete.
        Actor -> Plan complete!
    */
}
