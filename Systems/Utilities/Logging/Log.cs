using System;
using System.Threading;
using Godot;
using Microsoft.Extensions.Logging;
using Vikare.Utilities.Logging.Providers;

namespace Vikare.Utilities.Logging
{
    /// <summary> Stateless entry point for obtaining <see cref="ILogger"/> instances throughout the application. Owns a single <see cref="ILoggerFactory"/> that is initialised lazily on first use and lives for the process lifetime. </summary>
    public static class Log
    {
        /// <summary> Process-lifetime factory; initialised exactly once on first access. <see cref="LazyThreadSafetyMode.ExecutionAndPublication"/> ensures a single initialisation attempt across all threads. </summary>
        private static readonly Lazy<ILoggerFactory> _factory =
            new Lazy<ILoggerFactory>(BuildFactory, LazyThreadSafetyMode.ExecutionAndPublication);


        /// <summary> Returns an <see cref="ILogger"/> scoped to <typeparamref name="T"/>, using the fully-qualified type name as the category. </summary>
        /// <typeparam name="T"> Type whose full name is used as the log category. </typeparam>
        /// <returns> An <see cref="ILogger"/> for the specified type. </returns>
        public static ILogger For<T>() => _factory.Value.CreateLogger<T>();


        /// <summary> Returns an <see cref="ILogger"/> scoped to <paramref name="categoryName"/>, for cases where a type reference is not available (e.g. scripts, utilities). </summary>
        /// <param name="categoryName"> Arbitrary category string prepended to every log entry. </param>
        /// <returns> An <see cref="ILogger"/> for the specified category name. </returns>
        public static ILogger For(String categoryName) => _factory.Value.CreateLogger(categoryName);


        /// <summary> Registers a <see cref="UiLoggerProvider"/> against the running factory so that subsequently created loggers also write to <paramref name="target"/>.
        /// </summary>
        /// <param name="target"> The RichTextLabel node that should receive log output; must have BBCode enabled. </param>
        /// <remarks> Call this once the <see cref="RichTextLabel"/> is ready in the scene tree. Loggers resolved before this call will not receive UI output because MEL caches <see cref="ILogger"/> instances per category — resolve loggers after calling this method if UI output is required for a given category. </remarks>
        public static void AddUiSink(RichTextLabel target)
        {
            _factory.Value.AddProvider(new UiLoggerProvider(target));
        }


        /// <summary> Constructs and configures the <see cref="ILoggerFactory"/> with the default providers. </summary>
        /// <returns> A fully configured <see cref="ILoggerFactory"/>. </returns>
        /// <remarks> Minimum level is <see cref="LogLevel.Debug"/> in DEBUG builds and <see cref="LogLevel.Information"/> in all other configurations. </remarks>
        private static ILoggerFactory BuildFactory()
        {
            return LoggerFactory.Create(builder =>
            {
#if DEBUG
                builder.SetMinimumLevel(LogLevel.Debug);
#else
                builder.SetMinimumLevel(LogLevel.Information);
#endif
                builder.AddProvider(new GodotConsoleLoggerProvider());
                builder.AddProvider(new FileLoggerProvider("user://logs/vikare.log"));
            });
        }
    }
}
