using System;
using Microsoft.Extensions.Logging;

namespace Vikare.Utilities.Logging.Providers
{
    /// <summary> MEL provider that creates <see cref="GodotConsoleLogger"/> instances, routing log output to the Godot editor console. Register this provider via <c>ILoggingBuilder.AddProvider</c> during factory configuration. </summary>
    public sealed class GodotConsoleLoggerProvider : ILoggerProvider
    {
        /// <inheritdoc/>
        public ILogger CreateLogger(String categoryName) => new GodotConsoleLogger(categoryName);


        /// <inheritdoc/>
        public void Dispose() { }
    }
}
