using System;
using Microsoft.Extensions.Logging;

namespace Vikare.Utilities.Logging.Providers
{
    /// <summary> Shared base class for all Godot logging provider loggers. Implements the MEL contract members that are identical across every logger — <see cref="IsEnabled"/> and <see cref="BeginScope{TState}"/> — so subclasses only need to implement <see cref="Log{TState}"/> with their destination-specific formatting logic. </summary>
    public abstract class GodotLoggerBase : ILogger
    {
        /// <summary> Always returns <see langword="true"/>; filtering is delegated to the MEL pipeline configured on the owning <see cref="ILoggerFactory"/>. </summary>
        /// <param name="logLevel"> The level to evaluate; not inspected by this class. </param>
        /// <returns> Always <see langword="true"/>. </returns>
        public Boolean IsEnabled(LogLevel logLevel) => true;


        /// <summary> Returns the shared <see cref="NullScope.Instance"/>, satisfying the MEL contract without allocating a per-call scope object. </summary>
        /// <typeparam name="TState"> Type of the scope state; not used. </typeparam>
        /// <param name="state"> Scope state value; not used. </param>
        /// <returns> The shared <see cref="NullScope"/> singleton. </returns>
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => NullScope.Instance;


        /// <summary> Writes a log entry to this logger's backing destination. Implemented by each concrete subclass with destination-specific formatting and output. </summary>
        /// <typeparam name="TState"> Type of the structured state object carried by the entry. </typeparam>
        /// <param name="logLevel"> Severity of the entry, used for routing and colour selection. </param>
        /// <param name="eventId"> Optional event identifier; may be ignored by simple loggers. </param>
        /// <param name="state"> Structured state object holding the log parameters. </param>
        /// <param name="exception"> Optional exception to append to the formatted message. </param>
        /// <param name="formatter"> Delegate that produces the plain-text message from <paramref name="state"/> and <paramref name="exception"/>. </param>
        public abstract void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, String> formatter);
    }
}
