using Godot;
using Vikare.Entities.Combat;

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


    /// <summary> The actor desires to physically assault someone or something. </summary>
    public class AttackIntent : ActionIntent
    {
        /// <summary> The specific kind of attack being carried out. </summary>
        public AttackKind Kind { get; }


        /// <summary> The actor desires to physically assault someone or something. </summary>
        /// <param name="direction"> Desired movement direction in world space; expected to be normalised or <see cref="Vector2.Zero"/>. </param>
        /// <param name="kind"> The specific kind of attack being carried out. </param>
        public AttackIntent(Vector2 direction, AttackKind kind) : base(direction)
        {
            Kind = kind;
        }

        /// <summary> The type of physical attack embodied by the intent. </summary>
        public enum AttackKind
        {
            None = 0,
            Light = 1,
            Heavy = 2
        }
    }


    /// <summary> The actor desires to metaphysically assault someone or something. </summary>
    public class PowerIntent : ActionIntent
    {
        /// <summary> A kind of supernatural power being projected. </summary>
        public PowerDefinition Power { get; }


        /// <summary> The actor desires to metaphysically assault someone or something. </summary>
        /// <param name="direction"> Desired movement direction in world space; expected to be normalised or <see cref="Vector2.Zero"/>. </param>
        /// <param name="power"> A kind of supernatural power being projected. </param>
        public PowerIntent(Vector2 direction, PowerDefinition power) : base(direction)
        {
            Power = power;
        }
    }
}
