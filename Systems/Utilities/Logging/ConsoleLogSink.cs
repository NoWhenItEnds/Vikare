using System;
using System.Collections.Generic;
using Godot;

namespace Vikare.Utilities.Logging
{
    /// <summary> Routes log entries to the Godot editor console, using the appropriate Godot output method for each severity level. </summary>
    public class ConsoleLogSink : ILogSink
    {
        /// <inheritdoc/>
        public void Write(LogLevel level, String message, String source)
        {
            String formatted = LogFormatter.Format(level, source, message);

            switch(level)
            {
                case LogLevel.Debug:
                    GD.PrintRich($"[color=green]{formatted}[/color]");
                    break;
                case LogLevel.Info:
                    GD.PrintRich($"[color=cyan]{formatted}[/color]");
                    break;
                case LogLevel.Warn:
                    GD.PushWarning(formatted);
                    break;
                case LogLevel.Error:
                    GD.PushError(formatted);
                    break;
                default:
                    GD.PrintRich($"[color=purple]{formatted}[/color]");
                    break;
            }
        }
    }
}
