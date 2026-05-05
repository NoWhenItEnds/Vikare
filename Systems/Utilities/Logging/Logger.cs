using System;
using System.Collections.Generic;
using Vikare.Utilities.Singletons;

namespace Vikare.Utilities.Logging
{
    /// <summary>
    /// Singleton logging service that fans out each entry to every registered <see cref="ILogSink"/>.
    /// New output destinations are added by implementing <see cref="ILogSink"/> and calling
    /// <see cref="AddSink"/> — Logger itself never needs modification.
    ///
    /// Expected scene placement: one Logger node added as an autoload or early child of the scene root.
    /// </summary>
    public partial class Logger : SingletonNode<Logger>
    {
        /// <summary> Ordered collection of active sinks that each log entry is dispatched to. </summary>
        private readonly List<ILogSink> _sinks = new List<ILogSink>();


        /// <inheritdoc/>
        public override void _Ready()
        {
            AddSink(new ConsoleLogSink());
        }


        /// <summary> Registers a sink so it begins receiving all subsequent log entries. </summary>
        /// <param name="sink"> The sink to add; duplicates are silently ignored. </param>
        public void AddSink(ILogSink sink)
        {
            if (!_sinks.Contains(sink))
            {
                _sinks.Add(sink);
            }
        }


        /// <summary> Unregisters a sink so it no longer receives log entries. </summary>
        /// <param name="sink"> The sink to remove; no-op if not currently registered. </param>
        public void RemoveSink(ILogSink sink)
        {
            _sinks.Remove(sink);
        }


        /// <summary> Dispatches a debug-level entry to all registered sinks. Compiled out in non-debug builds. </summary>
        /// <param name="message"> Human-readable description of the event. </param>
        /// <param name="source"> Identifier of the system or node producing the entry. </param>
        [System.Diagnostics.Conditional("DEBUG")]
        public void Debug(String message, String source = "")
        {
            Dispatch(LogLevel.Debug, message, source);
        }


        /// <summary> Dispatches an info-level entry to all registered sinks. </summary>
        /// <param name="message"> Human-readable description of the event. </param>
        /// <param name="source"> Identifier of the system or node producing the entry. </param>
        public void Info(String message, String source = "")
        {
            Dispatch(LogLevel.Info, message, source);
        }


        /// <summary> Dispatches a warn-level entry to all registered sinks. </summary>
        /// <param name="message"> Human-readable description of the event. </param>
        /// <param name="source"> Identifier of the system or node producing the entry. </param>
        public void Warn(String message, String source = "")
        {
            Dispatch(LogLevel.Warn, message, source);
        }


        /// <summary> Dispatches an error-level entry to all registered sinks. </summary>
        /// <param name="message"> Human-readable description of the event. </param>
        /// <param name="source"> Identifier of the system or node producing the entry. </param>
        public void Error(String message, String source = "")
        {
            Dispatch(LogLevel.Error, message, source);
        }


        /// <summary> Iterates all registered sinks and calls <see cref="ILogSink.Write"/> on each. </summary>
        /// <param name="level"> Severity level to forward to each sink. </param>
        /// <param name="message"> The log message to forward. </param>
        /// <param name="source"> The originating system identifier to forward. </param>
        private void Dispatch(LogLevel level, String message, String source)
        {
            foreach (ILogSink sink in _sinks)
            {
                sink.Write(level, message, source);
            }
        }
    }
}
