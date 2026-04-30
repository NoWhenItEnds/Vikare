using System;
using Godot;

namespace Vikare.Entities.Abilities
{
    /// <summary> Abstract base resource for any ability that an actor can possess. </remarks>
    [GlobalClass]
    public abstract partial class AbilityEffect : Resource
    {
        /// <summary> Slot identity. Each concrete subclass overrides this with its fixed category. </summary>
        public abstract AbilityCategory Category { get; }


        /// <summary> Called when the actor begins using this ability. </summary>
        /// <param name="actor"> The owning actor. </param>
        public virtual void Enter(Actor actor) { }


        /// <summary> Called every visual frame while this ability is active. Use for visuals-only updates. </summary>
        /// <param name="actor"> The owning actor. </param>
        /// <param name="delta"> Elapsed time since the last visual frame, in seconds. </param>
        public virtual void Process(Actor actor, Double delta) { }


        /// <summary> Called every physics tick while this ability is active. Use for velocity writes and timers. </summary>
        /// <param name="actor"> The owning actor. </param>
        /// <param name="delta"> Elapsed time since the last physics tick, in seconds. </param>
        public virtual void PhysicsProcess(Actor actor, Double delta) { }


        /// <summary> Actually execute the affect. </summary>
        /// <param name="actor"> The owning actor. </param>
        public abstract void Execute(Actor actor);


        /// <summary> Called when the actor finishes using this ability. </summary>
        /// <param name="actor"> The owning actor. </param>
        public virtual void Exit(Actor actor) { }
    }
}
