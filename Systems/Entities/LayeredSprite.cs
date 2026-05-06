using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Vikare.Utilities.Extensions;
using Logger = Vikare.Utilities.Logging.Logger;

namespace Vikare.Entities
{
    /// <summary>
    /// An animated sprite composed of named <see cref="Sprite2D"/> layers that can have their textures
    /// swapped at runtime, enabling paper-doll style character visuals driven by a single
    /// shared <see cref="AnimationPlayer"/>.
    /// </summary>
    public partial class LayeredSprite : Node2D
    {
        /// <summary> Emitted when a new animation starts. </summary>
        [Signal] public delegate void AnimationStartedEventHandler();

        /// <summary> Emitted when the currently playing animation reaches its end. </summary>
        [Signal] public delegate void AnimationFinishedEventHandler();


        /// <summary> Whether an animation is currently playing. </summary>
        public Boolean IsPlaying { get; private set; } = false;

        /// <summary> The name of the animation that is currently playing. An empty string means that there isn't one. </summary>
        public String CurrentAnimation { get; private set; } = String.Empty;


        /// <summary> Lookup table of normalised part names to their <see cref="AnimatedSprite2D"/> nodes. </summary>
        private readonly Dictionary<String, AnimatedSprite2D> _parts = new Dictionary<String, AnimatedSprite2D>();

        /// <summary> References to all the sprite frames within the the project. The key is the graph find the resource (ENTITY_TYPE, RACE, PART, STATE, ANIMATION). </summary>
        /// <remarks> This map is shared across all instances of the LayeredSprite. </remarks>
        private static readonly Dictionary<String, SpriteFrames> SPRITE_FRAMES = ResourceExtensions.GetMappedResources<SpriteFrames>("res://Content/Resources/SpriteFrames");


        /// <inheritdoc/>
        public override void _Ready()
        {
            foreach (var frame in SPRITE_FRAMES)
            {
                GD.Print(frame.Key);
            }
            CacheParts();
        }


        /// <summary> Rebuilds <see cref="_parts"/> from every <see cref="AnimatedSprite2D"/> child currently in the scene tree. </summary>
        private void CacheParts()
        {
            _parts.Clear();

            foreach (AnimatedSprite2D sprite in GetChildren().OfType<AnimatedSprite2D>())
            {
                _parts[sprite.Name.ToLower()] = sprite;
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
                if (part.Key == "body") // TODO - Fix with mapping.
                {
                    String key = String.Join('.', [entityType, entityRace.ToLowerInvariant(), "body", "naked", animationName]);
                    if(SPRITE_FRAMES.TryGetValue(key, out SpriteFrames? frames) && frames != null)
                    {
                        String animationDirection = direction.ToDirection().ToString().ToLowerInvariant();
                        if(frames.HasAnimation(animationDirection))
                        {
                            part.Value.SpriteFrames = frames;
                            part.Value.Animation = animationDirection;
                            part.Value.Play();
                        }
                        else
                        {
                            Logger.Instance.Warn($"{frames.ResourcePath} doesn't have an animation called {animationDirection}.", Name);
                        }
                    }
                    else
                    {
                        Logger.Instance.Warn($"No animation found with the path '{key}'.", Name);
                    }
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


        /// <summary> Returns whether a part with the given name has been registered. </summary>
        /// <param name="partName"> The name of the part to check. </param>
        /// <returns> <c>true</c> if the part exists; otherwise <c>false</c>. </returns>
        public Boolean HasPart(String partName) => _parts.ContainsKey(partName.ToLowerInvariant());


        /// <summary> Shows or hides the named part. </summary>
        /// <param name="partName"> The name of the part to show or hide. </param>
        /// <param name="isVisible"> <c>true</c> to show the part; <c>false</c> to hide it. </param>
        /// <returns> Whether the texture was set or not. </returns>
        public Boolean TrySetPartVisible(String partName, Boolean isVisible)
        {
            Boolean isSuccessful = false;
            if (_parts.TryGetValue(partName.ToLowerInvariant(), out AnimatedSprite2D? sprite) && sprite != null)
            {
                sprite.Visible = isVisible;
                isSuccessful = true;
            }
            else
            {
                Logger.Instance.Error($"LayeredSprite: part '{partName}' not found in SetPartVisible.", Name);
            }
            return isSuccessful;
        }
    }
}
