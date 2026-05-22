using System;
using System.Collections;
using System.Collections.Specialized;
using System.Web;
using Moq;
using Xunit;

namespace Loom.Tests
{
    public class OrganizationResolverTests
    {
        private static (Mock<HttpContextBase> ctx, Hashtable items, NameValueCollection serverVars) BuildContext(
            string path,
            string serverVarSlug = null,
            string host = "loom.example.com",
            string scheme = "https")
        {
            var items = new Hashtable();
            var serverVars = new NameValueCollection();
            if (serverVarSlug != null)
                serverVars[OrganizationResolver.SlugServerVariable] = serverVarSlug;

            var url = new Uri($"{scheme}://{host}{path}");

            var request = new Mock<HttpRequestBase>();
            request.SetupGet(r => r.Url).Returns(url);
            request.SetupGet(r => r.ServerVariables).Returns(serverVars);

            var response = new Mock<HttpResponseBase>();
            response.Setup(r => r.Redirect(It.IsAny<string>(), It.IsAny<bool>()));

            var app = new Mock<HttpApplication>();

            var ctx = new Mock<HttpContextBase>();
            ctx.SetupGet(c => c.Items).Returns(items);
            ctx.SetupGet(c => c.Request).Returns(request.Object);
            ctx.SetupGet(c => c.Response).Returns(response.Object);
            ctx.SetupGet(c => c.ApplicationInstance).Returns(app.Object);

            return (ctx, items, serverVars);
        }

        [Fact]
        public void Resolve_stores_slug_and_settings_for_valid_tenant()
        {
            var (ctx, items, _) = BuildContext("/red", serverVarSlug: "red");

            OrganizationResolver.Resolve(ctx.Object);

            Assert.Equal("red", items[OrganizationResolver.SlugItemKey]);
            Assert.IsType<OrganizationSettings>(items[OrganizationResolver.SettingsItemKey]);
            var settings = (OrganizationSettings)items[OrganizationResolver.SettingsItemKey];
            Assert.Equal("red", settings.Slug);
        }

        [Fact]
        public void Resolve_is_short_circuited_when_slug_already_in_items()
        {
            var (ctx, items, _) = BuildContext("/red", serverVarSlug: "red");
            items[OrganizationResolver.SlugItemKey] = "pre-existing";

            OrganizationResolver.Resolve(ctx.Object);

            Assert.Equal("pre-existing", items[OrganizationResolver.SlugItemKey]);
            // Settings were not pre-cached.
            Assert.Null(items[OrganizationResolver.SettingsItemKey]);
        }

        [Fact]
        public void Resolve_throws_on_null_context()
        {
            Assert.Throws<ArgumentNullException>(() => OrganizationResolver.Resolve(null));
        }

        [Fact]
        public void Resolve_skips_when_first_segment_is_reserved_and_no_slug_set()
        {
            // /login is a reserved path. IIS would not have set ORGANIZATION_SLUG.
            var (ctx, items, _) = BuildContext("/login", serverVarSlug: null);

            OrganizationResolver.Resolve(ctx.Object);

            Assert.Null(items[OrganizationResolver.SlugItemKey]);
            Assert.Null(items[OrganizationResolver.SettingsItemKey]);
        }

        [Fact]
        public void Resolve_stores_slug_when_path_first_segment_looks_reserved_post_rewrite()
        {
            // Simulates IIS having rewritten /red/about -> /about and set ORGANIZATION_SLUG=red.
            // The reserved short-circuit must not fire because the slug is present.
            var (ctx, items, _) = BuildContext("/about", serverVarSlug: "red");

            OrganizationResolver.Resolve(ctx.Object);

            Assert.Equal("red", items[OrganizationResolver.SlugItemKey]);
            Assert.NotNull(items[OrganizationResolver.SettingsItemKey]);
        }

        [Fact]
        public void Resolve_redirects_when_slug_is_invalid()
        {
            var (ctx, items, _) = BuildContext("/not-a-tenant", serverVarSlug: "not-a-tenant");

            OrganizationResolver.Resolve(ctx.Object);

            // No slug stored — the redirect occurred.
            Assert.Null(items[OrganizationResolver.SlugItemKey]);
            Mock.Get(ctx.Object.Response).Verify(
                r => r.Redirect(It.Is<string>(s => s.Contains("context-invalid")), false),
                Times.Once);
        }

        [Fact]
        public void Resolve_redirects_to_context_missing_when_no_slug_and_no_subdomain_match()
        {
            // Host is "loom.example.com" which does not match the legacy subdomain pattern.
            var (ctx, items, _) = BuildContext("/", serverVarSlug: null, host: "loom.example.com");

            OrganizationResolver.Resolve(ctx.Object);

            Assert.Null(items[OrganizationResolver.SlugItemKey]);
            Mock.Get(ctx.Object.Response).Verify(
                r => r.Redirect(It.Is<string>(s => s.Contains("context-missing")), false),
                Times.Once);
        }

        [Fact]
        public void Resolve_resolves_mixed_case_slug_to_lowercase_tenant()
        {
            var (ctx, items, _) = BuildContext("/RED", serverVarSlug: "RED");

            OrganizationResolver.Resolve(ctx.Object);

            // Slug stored as supplied; settings resolved case-insensitively.
            Assert.Equal("RED", items[OrganizationResolver.SlugItemKey]);
            var settings = (OrganizationSettings)items[OrganizationResolver.SettingsItemKey];
            Assert.Equal("red", settings.Slug);
        }
    }
}
