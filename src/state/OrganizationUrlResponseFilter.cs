using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Loom
{
    /// <summary>
    /// Streaming HTML response filter that prepends the current tenant slug to
    /// root-relative <c>href</c>, <c>src</c>, and <c>action</c> URLs.
    ///
    /// Buffered text is flushed to the underlying stream whenever a safe boundary
    /// (the last '>' in the current buffer) is found and the buffer exceeds the
    /// flush threshold. Splitting at tag boundaries guarantees no attribute match
    /// is severed across a flush.
    /// </summary>
    public class OrganizationUrlResponseFilter : Stream
    {
        // Rewrite regex depends on the OrganizationCache. Built once at type load and
        // rebuilt whenever the cache is reloaded. Volatile so callers see the latest
        // pattern without locking.
        private static volatile Regex _rewritePattern = BuildRewritePattern();

        static OrganizationUrlResponseFilter()
        {
            OrganizationCache.Reloaded += () => _rewritePattern = BuildRewritePattern();
        }

        private static Regex BuildRewritePattern()
        {
            var allTenantSlugs = OrganizationCache.GetAll()
                .Select(o => o.Slug)
                .Concat(new[] { OrganizationCache.EmptySlug });

            var slugPattern = string.Join("|", allTenantSlugs.Select(Regex.Escape));

            // Match href="/...", src="/...", action="/..."
            // but NOT when URL starts with any tenant slug (with or without trailing slash)
            // and NOT protocol-relative URLs (//)
            return new Regex(
                @"(?<attr>href|src|action)=""(?<url>/(?!(" + slugPattern + @")(/|""|$)|/))",
                RegexOptions.Compiled | RegexOptions.IgnoreCase);
        }

        // Once the decoded buffer grows past this many chars, try to flush up to
        // the last '>' boundary. Keeps memory bounded for large responses.
        private const int FlushThresholdChars = 64 * 1024;

        private readonly Stream _responseStream;
        private readonly string _tenantSlug;
        private readonly Encoding _encoding;
        private readonly Decoder _decoder;
        private readonly StringBuilder _textBuffer = new StringBuilder();
        private bool _disposed;

        public OrganizationUrlResponseFilter(Stream responseStream, string tenantSlug, Encoding encoding)
        {
            _responseStream = responseStream;
            _tenantSlug = tenantSlug;
            _encoding = encoding ?? Encoding.UTF8;
            _decoder = _encoding.GetDecoder();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (count == 0) return;

            var maxChars = _encoding.GetMaxCharCount(count);
            var chars = new char[maxChars];
            var decoded = _decoder.GetChars(buffer, offset, count, chars, 0);

            _textBuffer.Append(chars, 0, decoded);

            if (_textBuffer.Length >= FlushThresholdChars)
            {
                FlushAtSafeBoundary();
            }
        }

        public override void Flush()
        {
            // Held until Dispose — rewriting requires the full attribute context.
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && !_disposed)
            {
                _disposed = true;

                // Drain any incomplete multi-byte sequence held by the decoder.
                var tail = new char[_encoding.GetMaxCharCount(0) + 16];
                var n = _decoder.GetChars(Array.Empty<byte>(), 0, 0, tail, 0, flush: true);

                if (n > 0)
                {
                    _textBuffer.Append(tail, 0, n);
                }

                if (_textBuffer.Length > 0)
                {
                    WriteRewritten(_textBuffer.ToString());
                    _textBuffer.Clear();
                }

                _responseStream.Dispose();
            }

            base.Dispose(disposing);
        }

        private void FlushAtSafeBoundary()
        {
            // Split at the last '>' so no attribute match straddles the boundary.
            var splitAt = -1;
            for (var i = _textBuffer.Length - 1; i >= 0; i--)
            {
                if (_textBuffer[i] == '>')
                {
                    splitAt = i + 1;
                    break;
                }
            }

            if (splitAt <= 0) return;

            var slice = _textBuffer.ToString(0, splitAt);

            WriteRewritten(slice);

            _textBuffer.Remove(0, splitAt);
        }

        private void WriteRewritten(string html)
        {
            var rewritten = _rewritePattern.Replace(html, m =>
                $"{m.Groups["attr"].Value}=\"/{_tenantSlug}{m.Groups["url"].Value}");

            var bytes = _encoding.GetBytes(rewritten);

            _responseStream.Write(bytes, 0, bytes.Length);
        }

        public override bool CanRead => false;
        public override bool CanSeek => false;
        public override bool CanWrite => true;
        public override long Length => throw new NotSupportedException();
        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }
        public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
    }
}
