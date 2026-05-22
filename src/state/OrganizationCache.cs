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
        /// Slugs that collide with built-in pages or routes. A tenant cannot be created
        /// with one of these slugs. Keep this list in sync with the IIS exclusion pattern
        /// in Web.config (OrganizationRootRewrite / OrganizationPathRewrite rules).
        /// </summary>
        public static readonly IReadOnlyCollection<string> ReservedSlugs =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "default",
                "about",
                "organizations",
                "context-missing",
                "context-invalid",
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

            Reloaded?.Invoke();
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
