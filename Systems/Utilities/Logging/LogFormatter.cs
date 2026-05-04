using System;

namespace Vikare.Utilities.Logging
{
    /// <summary>
    /// Produces the canonical plain-text log line shared by all sinks, ensuring a consistent
    /// format across every output destination without duplicating the format string.
    /// </summary>
    public static class LogFormatter
    {
        /// <summary> Combines level, source, and message into the standard <c>[Level] [Source] message</c> format. </summary>
        /// <param name="level"> Severity classification of the entry. </param>
        /// <param name="source"> Identifier of the system or node that produced the entry. </param>
        /// <param name="message"> Human-readable description of the event. </param>
        /// <returns> A formatted log string ready for output to any sink. </returns>
        public static String Format(LogLevel level, String source, String message)
        {
            return $"[{level}] [{source}] {message}";
        }
    }
}
