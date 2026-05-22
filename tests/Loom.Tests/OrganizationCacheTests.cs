using Loom;
using System.Collections.Generic;
using Xunit;

namespace Loom.Tests
{
    public class OrganizationCacheTests
    {
        [Theory]
        [InlineData("red")]
        [InlineData("orange")]
        [InlineData("yellow")]
        [InlineData("green")]
        [InlineData("blue")]
        [InlineData("indigo")]
        [InlineData("violet")]
        public void IsValidOrganization_returns_true_for_seed_slugs(string slug)
        {
            Assert.True(OrganizationCache.IsValidOrganization(slug));
        }

        [Theory]
        [InlineData("RED")]
        [InlineData("Red")]
        [InlineData("rEd")]
        public void IsValidOrganization_is_case_insensitive(string slug)
        {
            Assert.True(OrganizationCache.IsValidOrganization(slug));
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("empty")]
        [InlineData("login")]
        [InlineData("about")]
        [InlineData("default")]
        [InlineData("organizations")]
        [InlineData("healthz")]
        public void IsValidOrganization_rejects_empty_and_reserved(string slug)
        {
            Assert.False(OrganizationCache.IsValidOrganization(slug));
        }

        [Theory]
        [InlineData("does-not-exist")]
        [InlineData("not-a-tenant")]
        public void IsValidOrganization_rejects_unknown_slug(string slug)
        {
            Assert.False(OrganizationCache.IsValidOrganization(slug));
        }

        [Theory]
        [InlineData("My_Org")]
        [InlineData("-leading-hyphen")]
        [InlineData("trailing-hyphen-")]
        [InlineData("UPPERCASE")]
        [InlineData("white space")]
        [InlineData("dot.in.name")]
        public void IsValidSlugFormat_rejects_invalid_formats(string slug)
        {
            Assert.False(OrganizationCache.IsValidSlugFormat(slug));
        }

        [Theory]
        [InlineData("red")]
        [InlineData("a")]
        [InlineData("a-b")]
        [InlineData("a1b2c3")]
        [InlineData("my-very-long-but-valid-tenant-slug-name")]
        public void IsValidSlugFormat_accepts_canonical_formats(string slug)
        {
            Assert.True(OrganizationCache.IsValidSlugFormat(slug));
        }

        [Fact]
        public void IsValidSlugFormat_rejects_slug_over_39_chars()
        {
            var fortyChars = new string('a', 40);
            Assert.False(OrganizationCache.IsValidSlugFormat(fortyChars));
        }

        [Theory]
        [InlineData("login")]
        [InlineData("LOGIN")]
        [InlineData("Login")]
        public void IsReservedSlug_is_case_insensitive(string slug)
        {
            Assert.True(OrganizationCache.IsReservedSlug(slug));
        }

        [Fact]
        public void IsReservedSlug_rejects_non_reserved()
        {
            Assert.False(OrganizationCache.IsReservedSlug("red"));
            Assert.False(OrganizationCache.IsReservedSlug("does-not-exist"));
            Assert.False(OrganizationCache.IsReservedSlug(null));
        }

        [Fact]
        public void GetBySlug_returns_settings_for_seed()
        {
            var settings = OrganizationCache.GetBySlug("red");
            Assert.Equal("red", settings.Slug);
            Assert.Equal("Red Organization", settings.Name);
            Assert.Equal("red", settings.Color);
        }

        [Fact]
        public void GetBySlug_is_case_insensitive()
        {
            var settings = OrganizationCache.GetBySlug("RED");
            Assert.Equal("red", settings.Slug);
        }

        [Fact]
        public void GetBySlug_throws_for_unknown_slug()
        {
            Assert.Throws<KeyNotFoundException>(() => OrganizationCache.GetBySlug("does-not-exist"));
        }

        [Fact]
        public void GetAll_excludes_empty_slug()
        {
            var all = OrganizationCache.GetAll();
            Assert.DoesNotContain(all, x => x.Slug == OrganizationCache.EmptySlug);
        }

        [Fact]
        public void GetAll_returns_seed_tenants_in_name_order()
        {
            var all = OrganizationCache.GetAll();
            var names = new List<string>();
            foreach (var s in all) names.Add(s.Name);

            for (int i = 1; i < names.Count; i++)
            {
                Assert.True(string.CompareOrdinal(names[i - 1], names[i]) <= 0,
                    $"Expected '{names[i - 1]}' to be ordered before '{names[i]}'.");
            }
        }

        [Fact]
        public void Reload_replaces_snapshot_and_fires_event()
        {
            var called = 0;
            void Handler() => called++;

            OrganizationCache.Reloaded += Handler;
            try
            {
                OrganizationCache.Reload();
                Assert.Equal(1, called);
                Assert.True(OrganizationCache.IsValidOrganization("red"));
            }
            finally
            {
                OrganizationCache.Reloaded -= Handler;
            }
        }

        [Fact]
        public void Reload_continues_when_a_subscriber_throws()
        {
            var laterCalled = 0;
            void Throwing() { throw new System.InvalidOperationException("boom"); }
            void Later() => laterCalled++;

            OrganizationCache.Reloaded += Throwing;
            OrganizationCache.Reloaded += Later;
            try
            {
                OrganizationCache.Reload();
                Assert.Equal(1, laterCalled);
            }
            finally
            {
                OrganizationCache.Reloaded -= Throwing;
                OrganizationCache.Reloaded -= Later;
            }
        }

        [Fact]
        public void ReservedSlugs_contains_github_style_essentials()
        {
            Assert.Contains("login", OrganizationCache.ReservedSlugs);
            Assert.Contains("api", OrganizationCache.ReservedSlugs);
            Assert.Contains("healthz", OrganizationCache.ReservedSlugs);
            Assert.Contains("settings", OrganizationCache.ReservedSlugs);
        }
    }
}
