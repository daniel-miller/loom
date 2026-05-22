using System;
using System.Web;

namespace Loom.Diagnostics
{
    /// <summary>
    /// Per-request correlation identifier. Loom stamps one in
    /// <c>Application_BeginRequest</c>, propagates it through
    /// <c>HttpContext.Items</c>, and returns it to the client in the
    /// <c>X-Request-Id</c> response header.
    ///
    /// If the client supplies <c>X-Request-Id</c> on the inbound request and
    /// the value passes a safety check, that value is reused so the same
    /// identifier ties together logs across hops.
    /// </summary>
    public static class RequestId
    {
        public const string ItemKey = "Loom.RequestId";
        public const string HeaderName = "X-Request-Id";

        private const int MaxInboundLength = 64;

        /// <summary>
        /// Returns the existing per-request identifier, or generates one and stores
        /// it in <see cref="HttpContext.Items"/>.
        /// </summary>
        public static string GetOrCreate(HttpContextBase context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            if (context.Items[ItemKey] is string existing && !string.IsNullOrEmpty(existing))
                return existing;

            var fromClient = context.Request?.Headers?[HeaderName];
            var id = IsSafeInbound(fromClient)
                ? fromClient.Trim()
                : Guid.NewGuid().ToString("N");

            context.Items[ItemKey] = id;
            return id;
        }

        /// <summary>
        /// Returns the identifier stored on the current ambient request, or
        /// <c>null</c> if none was set.
        /// </summary>
        public static string Current
        {
            get
            {
                var ctx = HttpContext.Current;
                return ctx?.Items[ItemKey] as string;
            }
        }

        private static bool IsSafeInbound(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return false;

            var trimmed = id.Trim();
            if (trimmed.Length == 0 || trimmed.Length > MaxInboundLength) return false;

            // Allow only characters that are safe to echo back in a header value.
            foreach (var c in trimmed)
            {
                var ok = (c >= 'a' && c <= 'z')
                      || (c >= 'A' && c <= 'Z')
                      || (c >= '0' && c <= '9')
                      || c == '-' || c == '_';
                if (!ok) return false;
            }

            return true;
        }
    }
}
