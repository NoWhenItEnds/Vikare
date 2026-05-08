using Godot;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Vikare.Types;
using Vikare.Utilities.Extensions;
using Vikare.Utilities.Logging;

namespace Vikare.Entities
{
    /// <summary>
    /// An animated sprite composed of named <see cref="Sprite2D"/> layers that can have their textures
    /// swapped at runtime, enabling paper-doll style character visuals driven by a single
    /// shared <see cref="AnimationPlayer"/>.
    /// </summary>
    public partial class LayeredSprite : Node2D
    {
        /// <summary> Logger for animation resolution warnings. </summary>
        private static readonly ILogger Logger = Log.For<LayeredSprite>();

        /// <summary> Emitted when a new animation starts. </summary>
        [Signal] public delegate void AnimationStartedEventHandler();

        /// <summary> Emitted when the currently playing animation reaches its end. </summary>
        [Signal] public delegate void AnimationFinishedEventHandler();


        /// <summary> Whether an animation is currently playing. </summary>
        public Boolean IsPlaying { get; private set; } = false;


        /// <summary> Lookup table of normalised part names to their <see cref="AnimatedSprite2D"/> nodes. </summary>
        private readonly Dictionary<String, AnimatedSprite2D> _parts = new Dictionary<String, AnimatedSprite2D>();

        /// <summary> References to all the sprite frames within the the project. The key is the graph find the resource (ENTITY_TYPE, RACE, PART, STATE, ANIMATION). </summary>
        /// <remarks> This map is shared across all instances of the LayeredSprite. </remarks>
        private static readonly Dictionary<String, SpriteFrames> SPRITE_FRAMES = ResourceExtensions.GetMappedResources<SpriteFrames>("res://Content/Resources/SpriteFrames");

        private static readonly Dictionary<Direction, EntityPart[]> PART_ORDER = new Dictionary<Direction, EntityPart[]>()
        {
            //{Direction.Up, [EntityPartPosition.Back, EntityPartPosition.Legs, EntityPartPosition.Torso, EntityPartPosition.Head, EntityPartPosition.Hair]}
        };


        /// <inheritdoc/>
        public override void _Ready()
        {
            BuildParts();
        }


        /// <summary> Constructs all the sub-animated sprites and caches them. </summary>
        private void BuildParts()
        {
            foreach (EntityPart part in Enum.GetValues(typeof(EntityPart)))
            {
                if (part != EntityPart.None)
                {
                    String partName = part.ToString();
                    AnimatedSprite2D sprite = new AnimatedSprite2D();
                    AddChild(sprite);
                    sprite.Name = partName;
                    _parts[partName.ToLower()] = sprite;
                }
            }
        }


        /// <summary> Tries to play the given animation. </summary>
        /// <typeparam name="T"> The type of entity to play the animation for. </typeparam>
        /// <param name="entityRace"> The sub-kind of entity to play the animation for. </param>
        /// <param name="animationName"> The name of the animation being played. Also known as, what is the entity doing? </param>
        /// <param name="direction"> The direction the entity is currently facing. </param>
        public void PlayAnimation<T>(String entityRace, String animationName, Vector2 direction) where T : Entity  // TODO - Add mapping of part to state.
        {
            String entityType = typeof(T).Name.ToLowerInvariant();
            foreach (KeyValuePair<String, AnimatedSprite2D> part in _parts)
            {
                String key = String.Join('.', [entityType, entityRace.ToLowerInvariant(), part.Key, "naked", animationName]);   // TODO - Naked is default, uses clothing (or something) depending upon calling entity.
                if (SPRITE_FRAMES.TryGetValue(key, out SpriteFrames? frames) && frames != null)
                {
                    String animationDirection = direction.ToDirection().ToString().ToLowerInvariant();
                    if (frames.HasAnimation(animationDirection))
                    {
                        part.Value.SpriteFrames = frames;
                        part.Value.Animation = animationDirection;
                        part.Value.Play();
                    }
                    else
                    {
                        Logger.LogWarning("{ResourcePath} doesn't have an animation called {AnimationDirection}", frames.ResourcePath, animationDirection);
                    }
                }
                else
                {
                    // We will just silently let it fall through if there isn't a SpriteFrame for the specific combination for a part.
                    //Logger.LogWarning("No animation found with the path {Key}", key);
                }
            }

            IsPlaying = true;
            EmitSignal(SignalName.AnimationStarted);
        }


        /// <summary> Stops the currently playing animation immediately. </summary>
        public void StopAnimation()
        {
            foreach (KeyValuePair<String, AnimatedSprite2D> part in _parts)
            {
                part.Value.Stop();
            }

            IsPlaying = false;
            EmitSignal(SignalName.AnimationFinished);
        }
    }
}
