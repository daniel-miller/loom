using System;
using System.Threading;

namespace Loom.Diagnostics
{
    /// <summary>
    /// Ambient access point for <see cref="ILoomLogger"/>. Defaults to
    /// <see cref="TraceLogger"/>. Replace at startup via <see cref="Set"/>.
    /// </summary>
    public static class LoomLog
    {
        private static ILoomLogger _current = new TraceLogger();

        public static ILoomLogger Current => _current;

        /// <summary>
        /// Replaces the active logger. Passing <c>null</c> restores the default
        /// <see cref="TraceLogger"/>. Atomic via <see cref="Interlocked.Exchange{T}"/>.
        /// </summary>
        public static void Set(ILoomLogger logger)
        {
            Interlocked.Exchange(ref _current, logger ?? new TraceLogger());
        }
    }
}
