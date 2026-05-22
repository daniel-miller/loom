using System.IO;
using System.Text;
using Xunit;

namespace Loom.Tests
{
    public class OrganizationUrlResponseFilterTests
    {
        private static string RunFilter(string html, string tenantSlug, Encoding encoding = null)
        {
            encoding = encoding ?? Encoding.UTF8;
            var sink = new MemoryStream();
            var filter = new OrganizationUrlResponseFilter(sink, tenantSlug, encoding);

            var bytes = encoding.GetBytes(html);
            filter.Write(bytes, 0, bytes.Length);
            filter.Close();

            return encoding.GetString(sink.ToArray());
        }

        [Fact]
        public void Prefixes_root_relative_href_with_tenant_slug()
        {
            var input = "<a href=\"/about\">about</a>";
            var output = RunFilter(input, "red");
            Assert.Contains("href=\"/red/about\"", output);
        }

        [Fact]
        public void Prefixes_src_and_action_attributes()
        {
            var input = "<img src=\"/img/logo.png\" /><form action=\"/submit\"></form>";
            var output = RunFilter(input, "red");
            Assert.Contains("src=\"/red/img/logo.png\"", output);
            Assert.Contains("action=\"/red/submit\"", output);
        }

        [Fact]
        public void Does_not_prefix_urls_already_starting_with_tenant_slug()
        {
            var input = "<a href=\"/red/about\">about</a>";
            var output = RunFilter(input, "red");
            Assert.Contains("href=\"/red/about\"", output);
            Assert.DoesNotContain("/red/red/", output);
        }

        [Fact]
        public void Does_not_prefix_urls_starting_with_another_tenant_slug()
        {
            // /blue/foo is already tenant-prefixed for another tenant — leave alone.
            var input = "<a href=\"/blue/foo\">blue</a>";
            var output = RunFilter(input, "red");
            Assert.Contains("href=\"/blue/foo\"", output);
        }

        [Fact]
        public void Does_not_prefix_protocol_relative_urls()
        {
            var input = "<a href=\"//cdn.example.com/x\">cdn</a>";
            var output = RunFilter(input, "red");
            Assert.Contains("href=\"//cdn.example.com/x\"", output);
        }

        [Fact]
        public void Does_not_prefix_absolute_urls()
        {
            var input = "<a href=\"https://example.com/x\">ext</a>";
            var output = RunFilter(input, "red");
            Assert.Contains("href=\"https://example.com/x\"", output);
        }

        [Fact]
        public void Handles_chunked_writes_across_attribute_boundary()
        {
            var html = "<html><body><a href=\"/about\">about</a></body></html>";
            var bytes = Encoding.UTF8.GetBytes(html);
            var sink = new MemoryStream();
            var filter = new OrganizationUrlResponseFilter(sink, "red", Encoding.UTF8);

            // Write byte-by-byte to stress the streaming decoder + boundary split.
            for (var i = 0; i < bytes.Length; i++)
                filter.Write(bytes, i, 1);
            filter.Close();

            var output = Encoding.UTF8.GetString(sink.ToArray());
            Assert.Contains("href=\"/red/about\"", output);
        }

        [Fact]
        public void Honors_response_encoding()
        {
            var html = "<a href=\"/about\">about</a>";
            var output = RunFilter(html, "red", Encoding.Unicode);
            Assert.Contains("href=\"/red/about\"", output);
        }

        [Fact]
        public void Write_only_stream_throws_on_read_and_seek()
        {
            var filter = new OrganizationUrlResponseFilter(new MemoryStream(), "red", Encoding.UTF8);
            Assert.False(filter.CanRead);
            Assert.False(filter.CanSeek);
            Assert.True(filter.CanWrite);
            Assert.Throws<System.NotSupportedException>(() => filter.Read(new byte[1], 0, 1));
            Assert.Throws<System.NotSupportedException>(() => filter.Seek(0, SeekOrigin.Begin));
            Assert.Throws<System.NotSupportedException>(() => filter.SetLength(0));
        }
    }
}
