using System;
using System.Collections.Generic;
using Godot;

namespace Vikare.Utilities.Logging
{
    /// <summary> Routes log entries to the Godot editor console, using the appropriate Godot output method for each severity level. </summary>
    public class ConsoleLogSink : ILogSink
    {
        /// <summary>
        /// Maps each severity level to the corresponding Godot console action, eliminating
        /// conditional dispatch and allowing new levels to be supported by extending this table.
        /// </summary>
        private static readonly Dictionary<LogLevel, Action<String>> OutputActions = new Dictionary<LogLevel, Action<String>>
        {
            { LogLevel.Debug, GD.Print },
            { LogLevel.Info,  GD.Print },
            { LogLevel.Warn,  GD.PushWarning },
            { LogLevel.Error, GD.PushError },
        };


        /// <inheritdoc/>
        public void Write(LogLevel level, String message, String source)
        {
            String formatted = LogFormatter.Format(level, source, message);

            Action<String> action = OutputActions.TryGetValue(level, out Action<String>? mapped)
                ? mapped
                : GD.Print;

            action(formatted);
        }
    }
}
