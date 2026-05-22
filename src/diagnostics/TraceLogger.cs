using System;
using System.Diagnostics;

namespace Loom.Diagnostics
{
    /// <summary>
    /// Default <see cref="ILoomLogger"/> backed by <see cref="System.Diagnostics.Trace"/>.
    /// Acceptable for development. Replace with a structured logger in production.
    /// </summary>
    public sealed class TraceLogger : ILoomLogger
    {
        public void Info(string format, params object[] args)
        {
            Trace.TraceInformation(Format(format, args));
        }

        public void Warn(string format, params object[] args)
        {
            Trace.TraceWarning(Format(format, args));
        }

        public void Error(string format, Exception exception, params object[] args)
        {
            var msg = Format(format, args);
            if (exception != null)
                msg += Environment.NewLine + exception;

            Trace.TraceError("{0}", msg);
        }

        private static string Format(string format, object[] args)
        {
            if (string.IsNullOrEmpty(format)) return string.Empty;
            if (args == null || args.Length == 0) return format;
            return string.Format(format, args);
        }
    }
}
