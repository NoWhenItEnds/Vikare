using Godot;
using System;
using System.Collections.Generic;

namespace Vikare.Entities
{
    /// <summary> A animated sprite that works like a paper doll. </summary>
    public partial class LayeredSprite : Node2D
    {
        /// <summary> A reference to the animation player. </summary>
        [ExportGroup("Nodes")]
        [Export] private AnimationPlayer _animationPlayer = null!;


        /// <summary> Whether there is an animation currently playing. </summary>
        public Boolean IsPlaying => _animationPlayer.IsPlaying();

        /// <summary> The current animation that is being played. This is an empty string if none are. </summary>
        public String CurrentAnimation => _animationPlayer.CurrentAnimation;


        /// <summary> A map of the sprite nodes and the part's name. </summary>
        private Dictionary<String, Sprite2D> _parts = new Dictionary<String, Sprite2D>();


        /// <inheritdoc/>
        public override void _Ready()
        {
            CacheParts();
        }


        /// <summary> Cache all the body part nodes. </summary>
        private void CacheParts()
        {
            _parts.Clear();

            foreach (Node node in GetChildren())
            {
                if (node is Sprite2D sprite)
                {
                    _parts[node.Name.ToString().ToLower()] = sprite;
                }
            }
        }


        /// <summary> Play the given animation. </summary>
        /// <param name="libraryName"> The name of the animation library to search for. </param>
        /// <param name="animationName"> The name of the animation within the library to use. </param>
        public void PlayAnimation(String libraryName, String animationName)
        {
            AnimationLibrary? library = _animationPlayer.GetAnimationLibrary(libraryName);
            if(library != null)
            {
                Animation? animation = library.GetAnimation(animationName);
                if(animation != null)
                {
                    _animationPlayer.Play($"{libraryName}/{animationName}");
                }
            }
        }

        /// <summary> Set the texture of the given part. </summary>
        /// <param name="partName"> The name of the part to set. </param>
        /// <param name="texture"> The sprite to set. </param>
        public void SetPartTexture(String partName, Texture2D texture)
        {
            if (_parts.TryGetValue(partName.ToLower(), out Sprite2D? sprite) && sprite != null)
            {
                sprite.Texture = texture;
            }
        }


        /// <summary> Add a new body part to the sprite. </summary>
        /// <param name="partName"> The name of the part to add. </param>
        /// <param name="texture"> The initial texture of the new part. </param>
        public void AddPart(String partName, Texture2D texture)
        {
            Sprite2D sprite = new Sprite2D();
            sprite.Name = partName.ToLower();
            sprite.Texture = texture;

            AddChild(sprite);
            _parts[partName.ToLower()] = sprite;

            CacheParts();
        }


        /// <summary> Attempt to remove a body part. </summary>
        /// <param name="partName"> The name of the part to remove. </param>
        public void RemovePart(string partName)
        {
            if (_parts.TryGetValue(partName.ToLower(), out Sprite2D? sprite) && sprite != null)
            {
                sprite.QueueFree();
                _parts.Remove(partName.ToLower());
                CacheParts();
            }
        }
    }
}
