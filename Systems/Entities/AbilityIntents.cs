using Godot;
using Vikare.Entities.Abilities;

namespace Vikare.Entities
{
    /// <summary> A data-holder representing a "what the controller wants" message; both player and AI controllers produce these. </summary>
    public abstract class ActionIntent
    {
        /// <summary> Desired movement direction in world space; expected to be normalised or <see cref="Vector2.Zero"/>. </summary>
        public Vector2 Direction { get; }


        /// <summary> A data-holder representing a "what the controller wants" message; both player and AI controllers produce these. </summary>
        /// <param name="direction"> Desired movement direction in world space; expected to be normalised or <see cref="Vector2.Zero"/>. </param>
        public ActionIntent(Vector2 direction)
        {
            Direction = direction;
        }
    }


    /// <summary> The actor desires to wander in a particular direction. </summary>
    /// <param name="direction"> Desired movement direction in world space; expected to be normalised or <see cref="Vector2.Zero"/>. </param>
    public class WalkIntent(Vector2 direction) : ActionIntent(direction) { }


    /// <summary> The actor desires to move, rather quickly, in a particular direction. </summary>
    /// <param name="direction"> Desired movement direction in world space; expected to be normalised or <see cref="Vector2.Zero"/>. </param>
    public class SprintIntent(Vector2 direction) : ActionIntent(direction) { }


    /// <summary> The actor desires to duck and weave, in a particular direction. </summary>
    /// <param name="direction"> Desired movement direction in world space; expected to be normalised or <see cref="Vector2.Zero"/>. </param>
    public class DodgeIntent(Vector2 direction) : ActionIntent(direction) { }


    /// <summary> The actor desires to protect themselves from attacks coming from a particular direction. </summary>
    /// <param name="direction"> Desired movement direction in world space; expected to be normalised or <see cref="Vector2.Zero"/>. </param>
    public class BlockIntent(Vector2 direction) : ActionIntent(direction) { }


    /// <summary> The actor desires to use an ability (potentially on someone or something). </summary>
    public class AbilityIntent : ActionIntent
    {
        /// <summary> A kind of ability being projected. </summary>
        public AbilityEffect Effect { get; }


        /// <summary> The actor desires to use an ability (potentially on someone or something). </summary>
        /// <param name="direction"> Desired movement direction in world space; expected to be normalised or <see cref="Vector2.Zero"/>. </param>
        /// <param name="power"> A kind of ability being projected. </param>
        public AbilityIntent(Vector2 direction, AbilityEffect effect) : base(direction)
        {
            Effect = effect;
        }
    }
}
