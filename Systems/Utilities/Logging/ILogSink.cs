using System;

namespace Vikare.Utilities.Logging
{
    /// <summary> Contract for a destination that receives and persists formatted log entries. </summary>
    public interface ILogSink
    {
        /// <summary> Writes a single log entry to this sink's backing destination. </summary>
        /// <param name="level"> Severity classification for routing and filtering decisions. </param>
        /// <param name="message"> Human-readable description of the event. </param>
        /// <param name="source"> Identifier of the system or node that produced the entry. </param>
        void Write(LogLevel level, String message, String source);
    }
}
