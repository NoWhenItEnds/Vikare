using System;
using System.Collections.Generic;
using Godot;
using Microsoft.Extensions.Logging;

namespace Vikare.Utilities.Logging.Providers
{
    /// <summary> Writes log entries to the Godot editor console, mapping each MEL <see cref="LogLevel"/> to the appropriate Godot output method and BBCode colour. </summary>
    /// <remarks> Dispatch is driven by a static lookup table so new levels require only a new table entry; no conditional logic needs modification. </remarks>
    public class GodotConsoleLogger : GodotLoggerBase
    {
        /// <summary> Category name (typically the fully-qualified type name) prepended to every log line. </summary>
        private readonly String _categoryName;

        /// <summary> Maps each <see cref="LogLevel"/> to the Godot output action for that severity. Levels absent from the map produce no output (<see cref="LogLevel.None"/>). </summary>
        private static readonly IReadOnlyDictionary<LogLevel, Action<String>> Sinks =
            new Dictionary<LogLevel, Action<String>>
            {
                { LogLevel.Trace,       s => GD.PrintRich($"[color=light_grey]{s}[/color]") },
                { LogLevel.Debug,       s => GD.PrintRich($"[color=green]{s}[/color]") },
                { LogLevel.Information, s => GD.PrintRich($"[color=cyan]{s}[/color]") },
                { LogLevel.Warning,     s => GD.PushWarning(s) },
                { LogLevel.Error,       s => GD.PushError(s) },
                { LogLevel.Critical,    s => GD.PushError(s) },
            };


        /// <summary> Initialises the logger with the owning category name. </summary>
        /// <param name="categoryName"> Category identifier prepended to every formatted message. </param>
        public GodotConsoleLogger(String categoryName)
        {
            _categoryName = categoryName;
        }


        /// <inheritdoc/>
        public override void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, String> formatter)
        {
            String message = formatter(state, exception);
            String exceptionSuffix = exception != null ? $"\n{exception}" : String.Empty;
            String formatted = $"[{logLevel}] <{_categoryName}> {message}{exceptionSuffix}";

            if (Sinks.TryGetValue(logLevel, out Action<String>? sink))
            {
                sink(formatted);
            }
        }
    }
}
