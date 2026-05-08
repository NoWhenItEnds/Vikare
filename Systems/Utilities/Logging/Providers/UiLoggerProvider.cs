using System;
using Godot;
using Microsoft.Extensions.Logging;

namespace Vikare.Utilities.Logging.Providers
{
    /// <summary>
    /// MEL provider that creates <see cref="UiLogger"/> instances, routing log output
    /// to a <see cref="RichTextLabel"/> in the game's UI. Register this provider at
    /// runtime via <c>Log.AddUiSink(label)</c> once the label node is available.
    /// Note: loggers resolved from the factory before this provider is added will not
    /// receive UI output, as MEL caches logger instances per category. Resolve loggers
    /// after calling <c>Log.AddUiSink</c>, or accept console-only output for early
    /// startup messages.
    /// </summary>
    public sealed class UiLoggerProvider : ILoggerProvider
    {
        /// <summary> The RichTextLabel that all loggers created by this provider write to. </summary>
        private readonly RichTextLabel _target;


        /// <summary> Initialises the provider with the target label. </summary>
        /// <param name="target"> The label node that receives all UI log output; must have BBCode enabled. </param>
        public UiLoggerProvider(RichTextLabel target)
        {
            _target = target;
        }


        /// <inheritdoc/>
        public ILogger CreateLogger(String categoryName) => new UiLogger(categoryName, _target);


        /// <inheritdoc/>
        public void Dispose() { }
    }
}
