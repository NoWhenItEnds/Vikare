using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Vikare.Entities.Abilities;
using Vikare.Entities.States;

namespace Vikare.Entities
{
    /// <summary> An entity that is controlled by something, whether that is the player or an AI controller. </summary>
    public partial class Actor : Entity
    {
        /// <summary>State machine governing this actor's behaviour.</summary>
        [ExportGroup("Nodes")]
        [Export] public StateMachine Machine { get; private set; } = null!;

        /// <summary> The navigator node the agent uses to move through the world. </summary>
        [Export] public NavigationAgent2D NavigationAgent { get; private set; } = null!;


        /// <summary> Whether the entity is being controlled by the player, or an AI controller. </summary>
        [ExportGroup("Settings")]
        [Export] public Boolean IsPlayerControlled
        {
            get => _isPlayerControlled;
            set
            {
                _isPlayerControlled = value;
            }
        }


        /// <summary> The current direction the actor is facing. </summary>
        public Vector2 Direction { get; private set; } = Vector2.Down;

        /// <summary> Sets the velocity the actor wishes to travel at this frame and forwards it to <see cref="NavigationAgent"/> so the RVO2 avoidance solver can compute a safe velocity. </summary>
        /// <remarks> The actual <see cref="CharacterBody2D.Velocity"/> is written in <see cref="OnVelocityComputed"/>. </remarks>
        public Vector2 DesiredVelocity
        {
            set
            {
                if(_isPlayerControlled)
                {
                    Velocity = value;
                }
                else
                {
                    NavigationAgent.Velocity = value;
                }
            }
        }

        /// <summary> Whether the entity is being controlled by the player, or an AI controller. </summary>
        private Boolean _isPlayerControlled = false;

        /// <summary> The abilities currently possessed by the actor. </summary>
        private HashSet<AbilityEffect> _abilities = new HashSet<AbilityEffect>();


        /// <summary>
        /// Returns the Nth ability in <see cref="_abilities"/> that matches <paramref name="category"/>,
        /// where N is zero-based and determined by order of appearance. Returns null when no match exists.
        /// </summary>
        /// <param name="category">The category to search for.</param>
        /// <param name="index">Zero-based position among matching abilities; defaults to the first match.</param>
        public AbilityEffect? GetAbility(AbilityCategory category, Int32 index = 0)
        {
            AbilityEffect? result = GetAbilities(category)
                .ElementAtOrDefault(index);

            return result;
        }

        /// <summary>
        /// Returns all abilities in <see cref="_abilities"/> that match <paramref name="category"/>,
        /// in set order. Null slots (possible from partial editor assignment) are skipped.
        /// Uses lazy enumeration to avoid allocating a Godot collection.
        /// </summary>
        /// <param name="category">The category to filter by.</param>
        public IEnumerable<AbilityEffect> GetAbilities(AbilityCategory category)
        {
            return _abilities.Where(a => a != null && a.Category == category);
        }


        /// <inheritdoc/>
        public override void _Ready()
        {
            if (!NavigationAgent.AvoidanceEnabled)
            {
                throw new InvalidOperationException(
                    "Actor requires NavigationAgent2D.AvoidanceEnabled = true; movement is driven by the VelocityComputed signal and will never fire when avoidance is disabled.");
            }

            NavigationAgent.VelocityComputed += OnVelocityComputed;
        }


        /// <summary> Receives the avoidance-adjusted velocity from the RVO2 solver. </summary>
        /// <param name="safeVelocity">The obstacle-avoiding velocity computed by <see cref="NavigationAgent"/>.</param>
        private void OnVelocityComputed(Vector2 safeVelocity)
        {
            if (!IsPlayerControlled)
            {
                Velocity = safeVelocity;
            }
        }


        /// <inheritdoc/>
        public override void _PhysicsProcess(Double delta)
        {
            base._PhysicsProcess(delta);

            if (Velocity != Vector2.Zero)
            {
                Direction = Velocity.Normalized();  // Set the direction the actor is currently facing.
            }

            MoveAndSlide(); // End each frame with a slide.
        }



        /// <inheritdoc/>
        public override void PlayAnimation(String animationName, Vector2 direction)
        {
            Vector2 sanitisedDirection = direction != Vector2.Zero ? direction : Direction; // We don't want zero, so if the input is that, default to the player's current direction.
            Sprite.PlayAnimation<Actor>(Machine.Race, animationName, sanitisedDirection);
        }
    }
}
