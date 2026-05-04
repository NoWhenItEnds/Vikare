using System;
using System.Collections.Generic;
using Godot;

namespace Vikare.Utilities.Logging
{
    /// <summary>
    /// Appends BBCode-coloured log lines to a <see cref="RichTextLabel"/> node, maintaining a
    /// rolling window of at most <see cref="MaxLines"/> entries to prevent unbounded memory growth.
    /// </summary>
    public class UiLogSink : ILogSink
    {
        /// <summary> Maximum number of log lines retained in the label before the oldest line is removed. </summary>
        private const Int32 MaxLines = 200;

        /// <summary> The RichTextLabel node that receives appended log lines; must have BBCode enabled. </summary>
        public RichTextLabel? Target { get; set; }

        /// <summary>
        /// Ring buffer of raw BBCode lines, used to rebuild <see cref="Target"/> text after trimming.
        /// Stored as source BBCode because RichTextLabel.Text returns rendered plain-text, not the BBCode source.
        /// </summary>
        private readonly Queue<String> _lines = new Queue<String>();

        /// <summary>
        /// Maps each severity level to its BBCode colour name, eliminating conditional dispatch
        /// and allowing new levels to be supported by extending this table.
        /// </summary>
        private static readonly Dictionary<LogLevel, String> LevelColours = new Dictionary<LogLevel, String>
        {
            { LogLevel.Debug, "white" },
            { LogLevel.Info,  "green" },
            { LogLevel.Warn,  "yellow" },
            { LogLevel.Error, "red" },
        };


        /// <inheritdoc/>
        public void Write(LogLevel level, String message, String source)
        {
            if (Target == null)
            {
                GD.PushError("[UiLogSink] Target RichTextLabel is not assigned.");
            }
            else
            {
                String colour = LevelColours.TryGetValue(level, out String? mapped) ? mapped : "white";
                String formatted = LogFormatter.Format(level, source, message);
                String bbcodeLine = $"[color={colour}]{formatted}[/color]";

                _lines.Enqueue(bbcodeLine);

                if (_lines.Count > MaxLines)
                {
                    _lines.Dequeue();
                }

                Target.Clear();
                foreach (String line in _lines)
                {
                    Target.AppendText(line + "\n");
                }
            }
        }
    }
}
