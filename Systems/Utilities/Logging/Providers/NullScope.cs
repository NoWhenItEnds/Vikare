using System;

namespace Vikare.Utilities.Logging.Providers
{
    /// <summary> Shared no-op scope token returned by all <see cref="GodotLoggerBase"/> subclasses' <c>BeginScope</c> implementations. Held as a singleton to avoid per-call heap allocation. </summary>
    internal sealed class NullScope : IDisposable
    {
        /// <summary> Singleton instance shared across all logger instances to prevent repeated allocation. </summary>
        public static readonly NullScope Instance = new NullScope();


        /// <summary> Private constructor enforces use of <see cref="Instance"/>. </summary>
        private NullScope() { }


        /// <summary> Does nothing; exists only to satisfy <see cref="IDisposable"/>. </summary>
        public void Dispose() { }
    }
}
