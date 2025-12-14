using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Loom
{
    public class OrganizationUrlResponseFilter : Stream
    {
        private readonly Stream _responseStream;
        private readonly string _tenantSlug;
        private readonly MemoryStream _buffer = new MemoryStream();
        private readonly Regex _pattern;

        public OrganizationUrlResponseFilter(Stream responseStream, string tenantSlug)
        {
            _responseStream = responseStream;
            _tenantSlug = tenantSlug;

            // Match href="/...", src="/...", action="/...", etc.
            // when the URL is not already prefixed like href="/{tenant}/..."

            _pattern = new Regex(
                @"(?<attr>href|src|action)=""(?<url>/(?!" + Regex.Escape(tenantSlug) + @"/|/))",
                RegexOptions.Compiled | RegexOptions.IgnoreCase);
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
            var html = Encoding.UTF8.GetString(_buffer.ToArray());

            html = _pattern.Replace(html, m =>
                $"{m.Groups["attr"].Value}=\"/{_tenantSlug}{m.Groups["url"].Value}");

            var bytes = Encoding.UTF8.GetBytes(html);
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