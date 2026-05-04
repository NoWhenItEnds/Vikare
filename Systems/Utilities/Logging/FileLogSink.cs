using System;
using Godot;

namespace Vikare.Utilities.Logging
{
    /// <summary>
    /// Appends log entries to <c>user://logs/vikare.log</c>, opening and closing the file on every
    /// write so that log data is preserved even when the application crashes mid-session.
    /// </summary>
    public class FileLogSink : ILogSink
    {
        /// <summary> Virtual path to the log file inside Godot's user data directory. </summary>
        private const String LogPath = "user://logs/vikare.log";

        /// <summary> Virtual path to the directory containing the log file; created on first write if absent. </summary>
        private const String LogDirectory = "user://logs";

        /// <summary> Tracks whether the log directory has been confirmed or created this session, to avoid a filesystem call on every write. </summary>
        private Boolean _directoryReady;


        /// <inheritdoc/>
        public void Write(LogLevel level, String message, String source)
        {
            if (!_directoryReady)
            {
                EnsureLogDirectory();
            }

            String timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
            String formatted = LogFormatter.Format(level, source, message);
            String line = $"{timestamp} {formatted}\n";

            // Attempt to open in ReadWrite mode (preserves existing content, positions at start).
            // If the file does not yet exist, ReadWrite fails; fall back to Write which creates it.
            FileAccess? file = FileAccess.Open(LogPath, FileAccess.ModeFlags.ReadWrite);

            if (file == null)
            {
                file = FileAccess.Open(LogPath, FileAccess.ModeFlags.Write);
            }

            if (file == null)
            {
                Error openError = FileAccess.GetOpenError();
                GD.PushError($"[FileLogSink] Could not open '{LogPath}' for writing: {openError}");
            }
            else
            {
                file.SeekEnd();
                file.StoreString(line);
                file.Close();
            }
        }


        /// <summary>
        /// Creates <see cref="LogDirectory"/> if it does not already exist and sets <see cref="_directoryReady"/>;
        /// pushes a Godot error and leaves <see cref="_directoryReady"/> false if creation fails.
        /// </summary>
        private void EnsureLogDirectory()
        {
            Error mkdirError = DirAccess.MakeDirRecursiveAbsolute(LogDirectory);

            if (mkdirError == Error.Ok)
            {
                _directoryReady = true;
            }
            else
            {
                GD.PushError($"[FileLogSink] Could not create log directory '{LogDirectory}': {mkdirError}");
            }
        }
    }
}
