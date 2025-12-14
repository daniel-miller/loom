using System;
using System.Collections.Generic;
using System.Linq;

namespace Loom
{
    public static class OrganizationCache
    {
        public const string EmptySlug = "empty";

        private static Dictionary<string, OrganizationSettings> Organizations = new Dictionary<string, OrganizationSettings>();

        private static string[] Slugs = { "red", "orange", "yellow", "green", "blue", "indigo", "violet" };

        static OrganizationCache()
        {
            foreach (var slug in Slugs)
            {
                Add(slug);
            }

            Add(EmptySlug);
        }

        private static void Add(string slug)
        {
            var settings = new OrganizationSettings
            {
                Color = slug,

                Name = ToTitleCase(slug) + " Organization",

                Slug = slug
            };

            Organizations.Add(slug, settings);
        }

        private static string ToTitleCase(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            var words = input.Split(new[] { '-', '_' }, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < words.Length; i++)
            {
                if (words[i].Length > 0)
                    words[i] = char.ToUpper(words[i][0]) + words[i].Substring(1).ToLower();
            }

            return string.Join(" ", words);
        }

        public static OrganizationSettings[] GetAll()
        {
            return Organizations.Values
                .Where(x => x.Slug != EmptySlug)
                .OrderBy(x => x.Name)
                .ToArray();
        }

        public static OrganizationSettings GetBySlug(string slug)
        {
            if (Organizations.TryGetValue(slug, out var settings))
                return settings;

            throw new ArgumentOutOfRangeException(nameof(slug), $"Organization not found: {slug}");
        }

        public static bool IsValidOrganization(string slug)
        {
            return Slugs.Contains(slug);
        }
    }
}