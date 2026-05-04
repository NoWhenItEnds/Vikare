namespace Vikare.Utilities.Logging
{
    /// <summary> Severity levels used to classify log messages throughout the application. </summary>
    public enum LogLevel
    {
        /// <summary> Verbose diagnostic information only relevant during active development. </summary>
        Debug,

        /// <summary> General operational messages confirming expected behaviour. </summary>
        Info,

        /// <summary> Potentially harmful situations that did not prevent execution from continuing. </summary>
        Warn,

        /// <summary> Failures that have disrupted a specific operation and require attention. </summary>
        Error,
    }
}
