using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Loom
{
    /// <summary>
    /// In-memory cache of organizations. Replaceable at runtime via <see cref="Reload"/>.
    /// </summary>
    public static class OrganizationCache
    {
        public const string EmptySlug = "empty";

        /// <summary>
        /// Slugs that name app-scope pages or routes rather than tenants. A request to
        /// <c>/{reserved}</c> resolves in app-scope (no tenant context); a tenant cannot
        /// be onboarded with one of these slugs.
        ///
        /// Keep this list in sync with the <c>ReservedSlugs</c> rewriteMap in Web.config.
        /// Follows the GitHub convention of a single top-level namespace shared between
        /// tenants and app-scope paths, with a reserved-word list to disambiguate.
        /// </summary>
        public static readonly IReadOnlyCollection<string> ReservedSlugs =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                // Built-in pages and folders
                "default",
                "about",
                "organizations",
                "context-missing",
                "context-invalid",

                // Authentication / account
                "account",
                "auth",
                "forgot-password",
                "login",
                "logout",
                "oauth",
                "register",
                "reset-password",
                "sessions",
                "signin",
                "signup",
                "sso",

                // User-scope
                "dashboard",
                "messages",
                "notifications",
                "profile",
                "settings",
                "users",

                // App features
                "admin",
                "edit",
                "explore",
                "new",
                "search",
                "system",
                "trending",

                // Marketing
                "blog",
                "careers",
                "contact",
                "customer-stories",
                "developers",
                "docs",
                "enterprise",
                "faq",
                "features",
                "help",
                "jobs",
                "legal",
                "marketplace",
                "news",
                "partners",
                "plans",
                "press",
                "pricing",
                "privacy",
                "releases",
                "security",
                "support",
                "terms",
                "tour",

                // Operational
                "api",
                "healthz",
                "metrics",
                "ping",
                "status",

                // Static asset roots
                "assets",
                "css",
                "downloads",
                "fonts",
                "images",
                "img",
                "js",
                "media",
                "public",
                "static",
                "themes",

                // Crawler files (also matched by the IIS IsFile check, listed for safety)
                "favicon.ico",
                "robots.txt",
                "sitemap.xml",
            };

        // Seed data for the prototype. Production should pull this from a database
        // inside an IOrganizationLoader and swap the seed call in Reload().
        private static readonly string[] SeedSlugs =
            { "red", "orange", "yellow", "green", "blue", "indigo", "violet" };

        // Volatile reference so readers always see the latest snapshot after Reload.
        // Snapshots are immutable; mutation happens by replacing the whole reference.
        private static volatile ConcurrentDictionary<string, OrganizationSettings> _organizations = Load();

        /// <summary>
        /// Raised after the cache has been reloaded. Subscribers should rebuild any
        /// derived state that depends on the cache contents (regex patterns, etc.).
        /// </summary>
        public static event Action Reloaded;

        /// <summary>
        /// Replaces the in-memory snapshot with a freshly loaded copy. Atomic from a
        /// reader's perspective; readers see either the old or the new snapshot, never
        /// a half-built one.
        /// </summary>
        public static void Reload()
        {
            var fresh = Load();

            Interlocked.Exchange(ref _organizations, fresh);

            RaiseReloaded();
        }

        private static void RaiseReloaded()
        {
            var handlers = Reloaded;
            if (handlers == null) return;

            // Invoke subscribers individually so one throwing handler cannot prevent
            // the others from rebuilding their derived state.
            foreach (Action handler in handlers.GetInvocationList())
            {
                try
                {
                    handler();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.TraceError(
                        "OrganizationCache.Reloaded subscriber threw: {0}", ex);
                }
            }
        }

        public static OrganizationSettings[] GetAll()
        {
            return _organizations.Values
                .Where(x => x.Slug != EmptySlug)
                .OrderBy(x => x.Name)
                .ToArray();
        }

        public static OrganizationSettings GetBySlug(string slug)
        {
            if (_organizations.TryGetValue(slug, out var settings))
                return settings;

            throw new KeyNotFoundException($"Organization not found: {slug}");
        }

        public static bool IsValidOrganization(string slug)
        {
            if (string.IsNullOrEmpty(slug) || slug == EmptySlug)
                return false;

            if (IsReservedSlug(slug))
                return false;

            return _organizations.ContainsKey(slug);
        }

        public static bool IsReservedSlug(string slug)
        {
            return slug != null && ((HashSet<string>)ReservedSlugs).Contains(slug);
        }

        private static ConcurrentDictionary<string, OrganizationSettings> Load()
        {
            var dict = new ConcurrentDictionary<string, OrganizationSettings>();

            foreach (var slug in SeedSlugs)
            {
                if (IsReservedSlug(slug))
                    throw new InvalidOperationException(
                        $"Seed slug '{slug}' collides with a reserved path. Rename the seed or remove the reservation.");

                dict[slug] = Build(slug);
            }

            dict[EmptySlug] = Build(EmptySlug);

            return dict;
        }

        private static OrganizationSettings Build(string slug)
        {
            return new OrganizationSettings(
                slug: slug,
                name: ToTitleCase(slug) + " Organization",
                color: slug);
        }

        private static string ToTitleCase(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            var words = input.Split(new[] { '-', '_' }, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < words.Length; i++)
            {
                if (words[i].Length > 0)
                    words[i] = char.ToUpperInvariant(words[i][0]) + words[i].Substring(1).ToLowerInvariant();
            }

            return string.Join(" ", words);
        }
    }
}
