using System;
using System.Collections.Generic;
using Godot;
using Microsoft.Extensions.Logging;

namespace Vikare.Utilities.Logging.Providers
{
    /// <summary> Appends BBCode-coloured log lines to a <see cref="RichTextLabel"/> node, maintaining a rolling window of at most <see cref="MaxLines"/> entries. </summary>
    /// <remarks> All scene-tree mutations (<c>AppendText</c> and <c>RemoveParagraph</c>) are dispatched via <c>CallDeferred</c> so this logger is safe to call from any thread. </remarks>
    public class UiLogger : GodotLoggerBase
    {
        /// <summary> Maximum number of log paragraphs retained before the oldest is discarded. </summary>
        private const Int32 MaxLines = 200;

        /// <summary> Category name prepended to every formatted log line. </summary>
        private readonly String _categoryName;

        /// <summary> RichTextLabel that receives appended log lines; null if the provider was constructed without a valid target, in which case all writes are silent no-ops. </summary>
        private readonly RichTextLabel? _target;

        /// <summary> Maps each MEL <see cref="LogLevel"/> to the BBCode colour name used when rendering that level. </summary>
        private static readonly IReadOnlyDictionary<LogLevel, String> LevelColours =
            new Dictionary<LogLevel, String>
            {
                { LogLevel.Trace,       "white"  },
                { LogLevel.Debug,       "white"  },
                { LogLevel.Information, "green"  },
                { LogLevel.Warning,     "yellow" },
                { LogLevel.Error,       "red"    },
                { LogLevel.Critical,    "red"    },
            };


        /// <summary> Initialises the logger with its category name and the target label. </summary>
        /// <param name="categoryName"> Category identifier prepended to every formatted message. </param>
        /// <param name="target"> The <see cref="RichTextLabel"/> that receives log output; must have BBCode enabled. </param>
        public UiLogger(String categoryName, RichTextLabel? target)
        {
            _categoryName = categoryName;
            _target = target;
        }


        /// <inheritdoc/>
        public override void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, String> formatter)
        {
            if (_target != null)
            {
                String message = formatter(state, exception);
                String exceptionSuffix = exception != null ? $"\n{exception}" : String.Empty;
                String colour = LevelColours.TryGetValue(logLevel, out String? mapped) ? mapped : "white";
                String formatted = $"[color={colour}][{logLevel}] <{_categoryName}> {message}{exceptionSuffix}[/color]";

                if (_target.GetParagraphCount() >= MaxLines)
                {
                    _target.CallDeferred(RichTextLabel.MethodName.RemoveParagraph, 0);
                }

                _target.CallDeferred(RichTextLabel.MethodName.AppendText, formatted + "\n");
            }
        }
    }
}
