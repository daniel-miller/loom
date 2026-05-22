using System;

namespace Loom.Diagnostics
{
    /// <summary>
    /// Minimal logging surface used by Loom internals. The default implementation
    /// writes to <see cref="System.Diagnostics.Trace"/>. Production deployments
    /// replace it with a Serilog/NLog adapter via <see cref="LoomLog.Set"/>.
    /// </summary>
    public interface ILoomLogger
    {
        void Info(string format, params object[] args);
        void Warn(string format, params object[] args);
        void Error(string format, Exception exception, params object[] args);
    }
}
