using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Vikare.Entities.Components;
using Vikare.Entities.GOAP.Advertisers;

namespace Vikare.Entities
{
    /// <summary> Base class for all interactive entities within the game world. </summary>
    public abstract partial class Entity : CharacterBody2D
    {
        /// <summary> The entity's primary collision shape. </summary>
        [ExportGroup("Nodes")]
        [Export] public CollisionShape2D Collision { get; private set; } = null!;

        /// <summary> The sprite driving this entity's visual state. </summary>
        [Export] public LayeredSprite Sprite { get; private set; } = null!;


        /// <summary> An array of all the components on the current entity. </summary>
        private readonly HashSet<EntityComponent> _components = new HashSet<EntityComponent>();

        /// <summary> All advertisers attached to this entity; iterated by sensors to populate an actor's known-advertiser set. </summary>
        private readonly HashSet<ActionAdvertiser> _advertisers = new HashSet<ActionAdvertiser>();

        /// <summary> Exposes all advertisers on this entity for sensor iteration. </summary>
        public IReadOnlyCollection<ActionAdvertiser> Advertisers => _advertisers;


        /// <summary> Try to add a new component of the given type. </summary>
        /// <typeparam name="T"> The type of component to add. </typeparam>
        /// <returns> A reference to the newly created component, or null if a component of that type already exists. </returns>
        public T? TryAddComponent<T>() where T : EntityComponent, new()
        {
            T component = new T();
            Boolean added = _components.Add(component);

            if (added)
            {
                component.OnAddedTo(this);
            }

            return added ? component : null;
        }


        /// <summary> Try to add the instance of a component to the entity. </summary>
        /// <param name="component"> The component being added. </param>
        /// <returns> Whether the component was successfully added. </returns>
        /// <remarks> A deep copy of <paramref name="component"/> is stored — the original is not retained. </remarks>
        public Boolean TryAddComponent(EntityComponent component)
        {
            Boolean result = false;

            EntityComponent? newComponent = component.DuplicateDeep() as EntityComponent;
            if (newComponent != null)
            {
                result = _components.Add(newComponent);

                if (result)
                {
                    newComponent.OnAddedTo(this);
                }
            }

            return result;
        }


        /// <summary> Get the instance of the given component from the entity. </summary>
        /// <typeparam name="T"> The kind of component to get. </typeparam>
        /// <returns> A runtime reference to the component, or null if it doesn't exist. </returns>
        public T? GetComponent<T>() where T : EntityComponent => _components.OfType<T>().FirstOrDefault();


        /// <summary> Remove all components of the given type from the entity. </summary>
        /// <typeparam name="T"> The type of component to remove. </typeparam>
        /// <returns> True if at least one component was removed; false if none were found. </returns>
        public Boolean RemoveComponent<T>() where T : EntityComponent
        {
            List<EntityComponent> matches = _components.Where(x => x.GetType().Equals(typeof(T))).ToList();

            foreach (EntityComponent match in matches)
            {
                _components.Remove(match);
                match.OnRemovedFrom(this);
            }

            return matches.Count > 0;
        }


        /// <summary> Remove a specific instance of a component from the entity. </summary>
        /// <param name="component"> The component to remove. </param>
        /// <returns> Whether the component was successfully removed. </returns>
        public Boolean RemoveComponent(EntityComponent component)
        {
            Boolean removed = _components.Remove(component);

            if (removed)
            {
                component.OnRemovedFrom(this);
            }

            return removed;
        }


        /// <summary> Adds a specific advertiser instance to this entity. </summary>
        /// <param name="advertiser"> The advertiser to attach. </param>
        /// <returns> True if the advertiser was added; false if the same instance was already present. </returns>
        public Boolean TryAddAdvertiser(ActionAdvertiser advertiser) => _advertisers.Add(advertiser);


        /// <summary> Removes a specific advertiser instance from this entity. </summary>
        /// <param name="advertiser"> The advertiser to detach. </param>
        /// <returns> True if the advertiser was found and removed; false if it was not present. </returns>
        public Boolean RemoveAdvertiser(ActionAdvertiser advertiser) => _advertisers.Remove(advertiser);


        /// <summary> Removes all advertisers whose runtime type is exactly <typeparamref name="T"/>. </summary>
        /// <typeparam name="T"> The concrete advertiser type to remove. </typeparam>
        /// <returns> True if at least one advertiser was removed; false if none matched. </returns>
        public Boolean RemoveAdvertiser<T>() where T : ActionAdvertiser
        {
            List<ActionAdvertiser> matches = _advertisers.Where(x => x.GetType().Equals(typeof(T))).ToList();

            foreach (ActionAdvertiser match in matches)
            {
                _advertisers.Remove(match);
            }

            return matches.Count > 0;
        }


        /// <summary> Returns the first advertiser of type <typeparamref name="T"/> attached to this entity, or null if none is present. </summary>
        /// <typeparam name="T"> The advertiser type to find. </typeparam>
        /// <returns> The first matching advertiser, or null. </returns>
        public T? GetAdvertiser<T>() where T : ActionAdvertiser => _advertisers.OfType<T>().FirstOrDefault();


        /// <inheritdoc/>
        public override void _PhysicsProcess(double delta)
        {
            foreach (EntityComponent component in _components)
            {
                component.PhysicsProcess(delta);
            }
        }


        /// <summary> Plays the named animation. </summary>
        /// <param name="animationName"> The name of the animation to use. </param>
        /// <param name="direction"> The direction to use for the animation. </param>
        public abstract void PlayAnimation(String animationName, Vector2 direction);
    }
}
