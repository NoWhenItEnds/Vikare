using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
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
        /// <summary>
        /// Drives all layer animations; must be wired in the editor. Leaving this null causes
        /// a null-reference fault on the first animation or property access.
        /// </summary>
        [ExportGroup("Nodes")]
        [Export] private AnimationPlayer _animationPlayer = null!;


        /// <summary> Whether the animation player is currently running any clip. </summary>
        public Boolean IsPlaying => _animationPlayer.IsPlaying();

        /// <summary>
        /// The qualified name of the currently playing clip in <c>"library/clip"</c> format,
        /// or an empty string if nothing is playing.
        /// </summary>
        public String CurrentAnimation => _animationPlayer.CurrentAnimation;


        /// <summary> Emitted when the currently playing animation reaches its end. </summary>
        /// <param name="animationName"> The qualified <c>"library/clip"</c> name of the animation that finished. </param>
        [Signal] public delegate void AnimationFinishedEventHandler(String animationName);


        /// <summary> Lookup table of normalised part names to their <see cref="Sprite2D"/> nodes. </summary>
        private readonly Dictionary<String, Sprite2D> _parts = new Dictionary<String, Sprite2D>();


        /// <inheritdoc/>
        public override void _Ready()
        {
            CacheParts();
            _animationPlayer.AnimationFinished += OnAnimationPlayerFinished;
        }


        /// <summary> Rebuilds <see cref="_parts"/> from every <see cref="Sprite2D"/> child currently in the scene tree. </summary>
        private void CacheParts()
        {
            _parts.Clear();

            foreach (Sprite2D sprite in GetChildren().OfType<Sprite2D>())
            {
                _parts[NormaliseKey(sprite.Name)] = sprite;
            }
        }


        /// <summary> Returns a culture-invariant lowercase key used to index parts, avoiding Turkish-locale case-folding bugs. </summary>
        /// <param name="partName"> The raw part name to normalise. </param>
        /// <returns> The lowercase invariant form of <paramref name="partName"/>. </returns>
        private static String NormaliseKey(String partName) => partName.ToLowerInvariant();


        /// <summary>
        /// Plays the specified animation clip. Logs an error if the library or clip does not exist.
        /// When <paramref name="libraryName"/> is empty, the clip belongs to the default library and
        /// the animation player is addressed by clip name alone.
        /// </summary>
        /// <param name="libraryName"> The animation library name, or an empty string for the default library. </param>
        /// <param name="animationName"> The clip name within the library. </param>
        public void PlayAnimation(String libraryName, String animationName)
        {
            Boolean isDefaultLibrary = String.IsNullOrEmpty(libraryName);
            String qualifiedName = isDefaultLibrary ? animationName : $"{libraryName}/{animationName}";

            Boolean libraryExists = isDefaultLibrary || _animationPlayer.HasAnimationLibrary(libraryName);
            Boolean canPlay = libraryExists && _animationPlayer.HasAnimation(qualifiedName);

            if (canPlay)
            {
                _animationPlayer.Play(qualifiedName);
            }
            else
            {
                Logger.Instance.Error($"LayeredSprite: animation '{qualifiedName}' not found.", Name);
            }
        }


        /// <summary> Stops the currently playing animation immediately. </summary>
        public void StopAnimation()
        {
            _animationPlayer.Stop();
        }


        /// <summary> Returns whether a part with the given name has been registered. </summary>
        /// <param name="partName"> The name of the part to check. </param>
        /// <returns> <c>true</c> if the part exists; otherwise <c>false</c>. </returns>
        public Boolean HasPart(String partName) => _parts.ContainsKey(NormaliseKey(partName));


        /// <summary>
        /// Assigns a texture to the named part. Logs an error if the part does not exist.
        /// </summary>
        /// <param name="partName"> The name of the part to update. </param>
        /// <param name="texture"> The texture to assign to the part. </param>
        public void SetPartTexture(String partName, Texture2D texture)
        {
            String key = NormaliseKey(partName);

            if (_parts.TryGetValue(key, out Sprite2D? sprite) && sprite != null)
            {
                sprite.Texture = texture;
            }
            else
            {
                Logger.Instance.Error($"LayeredSprite: part '{partName}' not found in SetPartTexture.", Name);
            }
        }


        /// <summary>
        /// Returns the current texture of the named part, or <c>null</c> if the part does not exist.
        /// Logs an error on miss.
        /// </summary>
        /// <param name="partName"> The name of the part whose texture to retrieve. </param>
        /// <returns> The texture assigned to the part, or <c>null</c> if the part is not registered. </returns>
        public Texture2D? GetPartTexture(String partName)
        {
            String key = NormaliseKey(partName);
            Texture2D? result = null;

            if (_parts.TryGetValue(key, out Sprite2D? sprite) && sprite != null)
            {
                result = sprite.Texture;
            }
            else
            {
                Logger.Instance.Error($"LayeredSprite: part '{partName}' not found in GetPartTexture.", Name);
            }

            return result;
        }


        /// <summary>
        /// Shows or hides the named part. Logs an error if the part does not exist.
        /// </summary>
        /// <param name="partName"> The name of the part to show or hide. </param>
        /// <param name="visible"> <c>true</c> to show the part; <c>false</c> to hide it. </param>
        public void SetPartVisible(String partName, Boolean visible)
        {
            String key = NormaliseKey(partName);

            if (_parts.TryGetValue(key, out Sprite2D? sprite) && sprite != null)
            {
                sprite.Visible = visible;
            }
            else
            {
                Logger.Instance.Error($"LayeredSprite: part '{partName}' not found in SetPartVisible.", Name);
            }
        }


        /// <summary>
        /// Adds a new layer with the given name and texture. If a part with the same normalised name
        /// already exists, its node is freed and replaced cleanly with no orphaned nodes.
        /// </summary>
        /// <param name="partName"> The name for the new layer. </param>
        /// <param name="texture"> The initial texture for the new layer. </param>
        public void AddPart(String partName, Texture2D texture)
        {
            String key = NormaliseKey(partName);

            if (_parts.TryGetValue(key, out Sprite2D? existing) && existing != null)
            {
                RemoveChild(existing);
                existing.QueueFree();
            }

            Sprite2D sprite = new Sprite2D();
            sprite.Name = key;
            sprite.Texture = texture;

            AddChild(sprite);
            _parts[key] = sprite;
        }


        /// <summary> Removes the named part from the scene and the lookup table. Logs an error if the part does not exist. </summary>
        /// <param name="partName"> The name of the part to remove. </param>
        public void RemovePart(String partName)
        {
            String key = NormaliseKey(partName);

            if (_parts.TryGetValue(key, out Sprite2D? sprite) && sprite != null)
            {
                sprite.QueueFree();
                _parts.Remove(key);
            }
            else
            {
                Logger.Instance.Error($"LayeredSprite: part '{partName}' not found in RemovePart.", Name);
            }
        }


        /// <summary> Disconnects animation signals so the node can be safely freed. </summary>
        public override void _ExitTree()
        {
            _animationPlayer.AnimationFinished -= OnAnimationPlayerFinished;
        }


        /// <summary> Handles the <see cref="AnimationPlayer.AnimationFinished"/> signal and re-emits it as <see cref="AnimationFinished"/>. </summary>
        /// <param name="animName"> The qualified name of the animation that finished. </param>
        private void OnAnimationPlayerFinished(StringName animName)
        {
            EmitSignal(LayeredSprite.SignalName.AnimationFinished, animName.ToString());
        }
    }
}
