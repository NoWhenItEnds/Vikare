using System;
using Godot;
using Microsoft.Extensions.Logging;

namespace Vikare.Utilities.Logging.Providers
{
    /// <summary> MEL provider that writes log entries to a file, keeping the handle open for the lifetime of the provider so that each entry is appended efficiently. </summary>
    /// <remarks> Flushes after every write so logs survive a crash mid-session; the per-write flush is a deliberate trade-off against throughput. Do not remove it without replacing the crash-safety guarantee. </remarks>
    public sealed class FileLoggerProvider : ILoggerProvider
    {
        /// <summary> Virtual path to the directory that holds the log file; created on construction if absent. </summary>
        private const String LogDirectory = "user://logs";

        /// <summary> Object used to serialise file writes across threads. </summary>
        private readonly Object _lock = new Object();

        /// <summary> Open file handle; null when the file could not be opened. </summary>
        /// <remarks> Writes produce a single diagnostic error on the first attempt (see <see cref="_firstWriteWarned"/>) and are silent no-ops thereafter. </remarks>
        private FileAccess? _file;

        /// <summary> Guards against double-dispose. </summary>
        private Boolean _disposed;

        /// <summary> Ensures the "file logging disabled" warning is emitted at most once, on the first write attempt after a failed open, rather than on every subsequent write. </summary>
        private Boolean _firstWriteWarned;

        /// <summary> Virtual path to the log file, supplied by the caller. </summary>
        private readonly String _filePath;


        /// <summary> Creates the log directory if absent, then opens <paramref name="filePath"/> for appending. </summary>
        /// <param name="filePath"> Godot virtual path (e.g. <c>user://logs/vikare.log</c>) of the target log file. </param>
        public FileLoggerProvider(String filePath)
        {
            _filePath = filePath;
            OpenFile();
        }


        /// <inheritdoc/>
        public ILogger CreateLogger(String categoryName) => new FileLogger(categoryName, this);


        /// <summary> Appends <paramref name="line"/> to the open file and flushes the buffer so the entry survives a crash. Thread-safe via an internal lock. </summary>
        /// <param name="line"> The fully-formatted log line including trailing newline. </param>
        public void WriteLine(String line)
        {
            lock (_lock)
            {
                if (_file != null)
                {
                    _file.StoreString(line);
                    _file.Flush();
                }
                else if (!_firstWriteWarned)
                {
                    GD.PushError("[FileLoggerProvider] File logging is disabled because the log file could not be opened — see earlier error for details.");
                    _firstWriteWarned = true;
                }
            }
        }


        /// <summary> Flushes and closes the file handle. Safe to call multiple times. </summary>
        public void Dispose()
        {
            if (!_disposed)
            {
                lock (_lock)
                {
                    if (_file != null)
                    {
                        _file.Flush();
                        _file.Close();
                        _file = null;
                    }
                }

                _disposed = true;
            }
        }


        /// <summary> Creates <see cref="LogDirectory"/> if absent, then attempts to open the log file in <c>ReadWrite</c> mode (preserving existing content) with <c>SeekEnd</c> for appending. Falls back to <c>Write</c> mode if the file does not yet exist. </summary>
        private void OpenFile()
        {
            Error mkdirError = DirAccess.MakeDirRecursiveAbsolute(LogDirectory);

            if (mkdirError != Error.Ok)
            {
                GD.PushError($"[FileLoggerProvider] Could not create log directory '{LogDirectory}': {mkdirError}");
            }
            else
            {
                FileAccess? handle = FileAccess.Open(_filePath, FileAccess.ModeFlags.ReadWrite);

                if (handle == null)
                {
                    handle = FileAccess.Open(_filePath, FileAccess.ModeFlags.Write);
                }

                if (handle == null)
                {
                    Error openError = FileAccess.GetOpenError();
                    GD.PushError($"[FileLoggerProvider] Could not open '{_filePath}' for writing: {openError}");
                }
                else
                {
                    handle.SeekEnd();
                    _file = handle;
                }
            }
        }
    }
}
