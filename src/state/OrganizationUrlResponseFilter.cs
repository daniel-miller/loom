using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Loom
{
    public class OrganizationUrlResponseFilter : Stream
    {
        // Slug list and rewrite regex depend only on the OrganizationCache, which is
        // static-initialized. Build once at type load instead of per request.
        private static readonly Regex RewritePattern = BuildRewritePattern();

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

        private readonly Stream _responseStream;
        private readonly string _tenantSlug;
        private readonly Encoding _encoding;
        private readonly MemoryStream _buffer = new MemoryStream();

        public OrganizationUrlResponseFilter(Stream responseStream, string tenantSlug, Encoding encoding)
        {
            _responseStream = responseStream;
            _tenantSlug = tenantSlug;
            _encoding = encoding ?? Encoding.UTF8;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            _buffer.Write(buffer, offset, count);
        }

        public override void Flush()
        {
            // Don't flush to underlying stream yet
        }

        public override void Close()
        {
            if (_buffer.Length == 0)
            {
                _responseStream.Close();
                return;
            }

            _buffer.Position = 0;
            var html = _encoding.GetString(_buffer.ToArray());

            html = RewritePattern.Replace(html, m =>
                $"{m.Groups["attr"].Value}=\"/{_tenantSlug}{m.Groups["url"].Value}");

            var bytes = _encoding.GetBytes(html);
            _responseStream.Write(bytes, 0, bytes.Length);
            _responseStream.Close();
        }

        public override bool CanRead => false;
        public override bool CanSeek => false;
        public override bool CanWrite => true;
        public override long Length => _buffer.Length;
        public override long Position
        {
            get => _buffer.Position;
            set => _buffer.Position = value;
        }
        public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();

    }
}