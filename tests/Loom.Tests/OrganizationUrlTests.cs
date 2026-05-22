using System;
using System.Collections;
using System.Web;
using Moq;
using Xunit;

namespace Loom.Tests
{
    public class OrganizationUrlTests
    {
        private static HttpContextBase ContextWithSlug(string slug)
        {
            var items = new Hashtable();
            if (slug != null)
                items[OrganizationResolver.SlugItemKey] = slug;

            var ctx = new Mock<HttpContextBase>();
            ctx.SetupGet(c => c.Items).Returns(items);
            return ctx.Object;
        }

        [Fact]
        public void Resolve_uses_explicit_slug_when_provided()
        {
            var ctx = ContextWithSlug("red");
            var url = OrganizationUrl.Resolve(ctx, "~/reports", "blue");
            Assert.Equal("/blue/reports", url);
        }

        [Fact]
        public void Resolve_reads_slug_from_context_when_omitted()
        {
            var ctx = ContextWithSlug("red");
            var url = OrganizationUrl.Resolve(ctx, "~/reports");
            Assert.Equal("/red/reports", url);
        }

        [Fact]
        public void Resolve_strips_leading_tilde_and_slash()
        {
            var ctx = ContextWithSlug("red");
            Assert.Equal("/red/reports", OrganizationUrl.Resolve(ctx, "~/reports"));
            Assert.Equal("/red/reports", OrganizationUrl.Resolve(ctx, "/reports"));
            Assert.Equal("/red/reports", OrganizationUrl.Resolve(ctx, "reports"));
        }

        [Fact]
        public void Resolve_preserves_query_string()
        {
            var ctx = ContextWithSlug("red");
            var url = OrganizationUrl.Resolve(ctx, "~/reports?id=1&sort=desc");
            Assert.Equal("/red/reports?id=1&sort=desc", url);
        }

        [Fact]
        public void Resolve_throws_when_relativePath_is_null()
        {
            var ctx = ContextWithSlug("red");
            Assert.Throws<ArgumentNullException>(() => OrganizationUrl.Resolve(ctx, null));
        }

        [Fact]
        public void Resolve_throws_when_context_is_null_and_no_explicit_slug()
        {
            Assert.Throws<ArgumentNullException>(() => OrganizationUrl.Resolve(null, "~/reports"));
        }

        [Fact]
        public void Resolve_accepts_null_context_when_explicit_slug_supplied()
        {
            var url = OrganizationUrl.Resolve(null, "~/reports", "blue");
            Assert.Equal("/blue/reports", url);
        }
    }
}
