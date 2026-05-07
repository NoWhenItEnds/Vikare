using System;

namespace Vikare.Entities.GOAP
{
    /// <summary> A named boolean condition about the world, evaluated lazily at planning time. </summary>
    public class ActorFact : IEquatable<ActorFact>
    {
        /// <summary> The identifying key used for equality, hashing, and dictionary lookup. </summary>
        public String Name { get; private set; }

        /// <summary> The delegate that determines the fact's current truth value. </summary>
        private Func<Boolean>? _evaluator;


        /// <summary> Creates a named fact with no evaluator set. </summary>
        /// <param name="name"> The identifying key for this fact. </param>
        private ActorFact(String name)
        {
            Name = name;
        }


        /// <summary> Invokes the configured evaluator and returns the current truth value. </summary>
        /// <returns> The evaluator's result, or false if no evaluator is configured. </returns>
        public Boolean Evaluate()
        {
            Boolean result;

            if (_evaluator == null)
            {
                result = false;
            }
            else
            {
                result = _evaluator();
            }

            return result;
        }


        /// <inheritdoc/>
        public override Int32 GetHashCode() => HashCode.Combine(Name);


        /// <inheritdoc/>
        public override Boolean Equals(Object? obj)
        {
            ActorFact? other = obj as ActorFact;
            return other != null && Name.Equals(other.Name, StringComparison.Ordinal);
        }


        /// <inheritdoc/>
        public Boolean Equals(ActorFact? other) => other != null && Name.Equals(other.Name, StringComparison.Ordinal);


        /// <summary> A builder for constructing facts before registration with the controller. </summary>
        public class Builder
        {
            /// <summary> The fact under construction. </summary>
            private readonly ActorFact _fact;


            /// <summary> Creates a builder for a fact with the given name. </summary>
            /// <param name="name"> The identifying key for the fact. </param>
            public Builder(String name)
            {
                _fact = new ActorFact(name);
            }


            /// <summary> Sets the evaluator delegate; replaces any previously set evaluator. </summary>
            /// <param name="evaluator"> The delegate invoked at planning time to determine truth. </param>
            public Builder WithCondition(Func<Boolean> evaluator)
            {
                _fact._evaluator = evaluator;
                return this;
            }


            /// <summary> Returns the configured fact. </summary>
            /// <returns> The newly constructed fact, ready for registration. </returns>
            public ActorFact Build()
            {
                return _fact;
            }
        }
    }
}
