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

        /// <summary> Animation player driving this entity's visual state. </summary>
        [Export] public AnimationPlayer AnimationPlayer { get; private set; } = null!;


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


        public T? TryAddComponent<T>(T component) where T : EntityComponent
        {
            Boolean result = false;

            T? newComponent = component.DuplicateDeep() as T;
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


        public void RemoveComponent<T>() where T : EntityComponent => _components.RemoveWhere(x => x.GetType().Equals(typeof(T)));


        /// <summary> Plays the named animation clip. </summary>
        /// <param name="animationName"> Clip name as defined in the animation library. </param>
        /// <remarks> No-op if that clip is already playing, preventing looping animations from restarting each tick. </remarks>
        public void PlayAnimation(String animationName)
        {
            bool alreadyPlaying = AnimationPlayer.CurrentAnimation == animationName
                && AnimationPlayer.IsPlaying();

            if (!alreadyPlaying)
            {
                AnimationPlayer.Play(animationName);
            }
        }
    }
}
