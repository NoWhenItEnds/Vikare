using Vikare.Entities.Interfaces;

namespace Vikare.Entities.Intents
{
    /// <summary>
    /// An <see cref="IInputIntent"/> expressing a one-shot request to dodge.
    /// Carries no direction payload — the dodge state snapshots the current movement velocity on entry, so the
    /// controller only needs to signal that a dodge was requested, not where. A direction hint field was considered
    /// but omitted to keep the intent a pure trigger, consistent with the single-responsibility of <see cref="MoveIntent"/>.
    /// </summary>
    public sealed class DodgeIntent : IInputIntent
    {
    }
}
