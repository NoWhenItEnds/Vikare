using System;
using Godot;

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
