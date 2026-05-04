using System;
using Godot;

namespace Vikare.Entities.Abilities
{
    /// <summary> Abstract base resource for any ability that an actor can possess. </summary>
    /// <remarks> A specific instance of an ability is shared between all its users, so it cannot track state for a specific entity. </remarks>
    [GlobalClass]
    public abstract partial class AbilityEffect : Resource
    {
        /// <summary> Clip name that the actor's AnimationPlayer plays when this ability activates. </summary>
        [ExportGroup("Settings")]
        [Export] public String AnimationName { get; set; } = String.Empty;


        /// <summary> Slot identity. Each concrete subclass overrides this with its fixed category. </summary>
        public abstract AbilityCategory Category { get; }


        /// <summary> Called every visual frame whilst this ability is active. Use for visuals-only updates. </summary>
        /// <param name="delta"> Elapsed time since the last visual frame, in seconds. </param>
        /// <param name="actor"> The entity currently performing this ability. </param>
        public virtual void Process(Double delta, Actor actor) { }


        /// <summary> Called every physics tick whilst this ability is active. Use for velocity writes. </summary>
        /// <param name="delta"> Elapsed time since the last physics tick, in seconds. </param>
        /// <param name="actor"> The entity currently performing this ability. </param>
        public virtual void PhysicsProcess(Double delta, Actor actor) { }


        /// <summary> Applies the ability's effect at the hit frame indicated by the animation method-call track. </summary>
        /// <param name="actor"> The owning actor. </param>
        public abstract void Execute(Actor actor);
    }
}
