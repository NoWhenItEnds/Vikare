using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Vikare.Entities.Components;

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
        private HashSet<EntityComponent> _components = new HashSet<EntityComponent>();


        /// <summary> Try to add a new component of the given type. </summary>
        /// <typeparam name="T"> The type of component to add. </typeparam>
        /// <returns> A reference to the newly created component. </returns>
        public T? TryAddComponent<T>() where T : EntityComponent, new()
        {
            T component = new T();
            Boolean result = _components.Add(component);
            return result ? component : null;
        }


        /// <summary> Try to add the instance of a component to the entity. </summary>
        /// <param name="component"> The component being added. </param>
        /// <returns> Whether the component was successfully added. </returns>
        public Boolean TryAddComponent(EntityComponent component)
        {
            Boolean result = false;

            EntityComponent? newComponent = component.DuplicateDeep() as EntityComponent;
            if(newComponent != null)
            {
                result = _components.Add(newComponent);
            }

            return result;
        }


        /// <summary> Get the instance of the given component from the entity. </summary>
        /// <typeparam name="T"> The kind of component to get. </typeparam>
        /// <returns> A runtime reference to the component, or null if it doesn't exist. </returns>
        public T? GetComponent<T>() where T : EntityComponent => _components.OfType<T>().FirstOrDefault();


        /// <summary> Remove a type of component from the entity. </summary>
        /// <typeparam name="T"> The type of component to remove. </typeparam>
        /// <returns> Whether the component was successfully removed. </returns>
        public Boolean RemoveComponent<T>() where T : EntityComponent => _components.RemoveWhere(x => x.GetType().Equals(typeof(T))) > 0;


        /// <summary> Remove a specific instance of a component from the entity. </summary>
        /// <param name="component"> The component to remove. </param>
        /// <returns> Whether the component was successfully removed. </returns>
        public Boolean RemoveComponent(EntityComponent component) => _components.Remove(component);


        /// <summary> Plays the named animation clip. </summary>
        /// <param name="libraryName"> The name of the animation library to search for. </param>
        /// <param name="animationName"> The name of the animation within the library to use. </param>
        /// <remarks> No-op if that clip is already playing, preventing looping animations from restarting each tick. </remarks>
        public void PlayAnimation(String libraryName, String animationName)
        {
            Boolean alreadyPlaying = Sprite.CurrentAnimation == animationName && Sprite.IsPlaying;
            if (!alreadyPlaying)
            {
                Sprite.PlayAnimation(libraryName, animationName);
            }
        }
    }
}
