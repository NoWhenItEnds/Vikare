using System;
using Microsoft.Extensions.Logging;

namespace Vikare.Utilities.Logging.Providers
{
    /// <summary> Writes log entries to the file handle owned by the parent <see cref="FileLoggerProvider"/>, prepending a UTC timestamp to each line. </summary>
    /// <remarks> All file operations are guarded by the provider's shared lock to ensure cross-thread safety (Godot's <c>FileAccess</c> is not documented as thread-safe). </remarks>
    public class FileLogger : GodotLoggerBase
    {
        /// <summary> Category name prepended to every formatted log line. </summary>
        private readonly String _categoryName;

        /// <summary> Parent provider that owns the open file handle and synchronisation lock. </summary>
        private readonly FileLoggerProvider _provider;


        /// <summary> Initialises the logger with its owning category name and provider. </summary>
        /// <param name="categoryName"> Category identifier prepended to every formatted message. </param>
        /// <param name="provider"> Provider that owns the file handle and lock used for writes. </param>
        public FileLogger(String categoryName, FileLoggerProvider provider)
        {
            _categoryName = categoryName;
            _provider = provider;
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
            String timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
            String line = $"{timestamp} [{logLevel}] <{_categoryName}> {message}{exceptionSuffix}\n";
            _provider.WriteLine(line);
        }
    }
}
