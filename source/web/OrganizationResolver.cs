using System.Web;

namespace Loom
{
    /// <summary>
    /// Determines the organization account (tenant) for the current HTTP request.
    /// </summary>
    public class OrganizationResolver
    {
        public const string SlugVariable = "ORGANIZATION_SLUG";

        public const string SettingsVariable = "ORGANIZATION_SETTINGS";

        public static void Resolve(HttpRequest request, HttpResponse response)
        {
            // Already resolved in this request cycle (internal rewrite)
            if (HttpContext.Current.Items.Contains(SlugVariable))
                return;

            var slug = request.ServerVariables[SlugVariable];

            if (string.IsNullOrEmpty(slug))
            {
                var url = OrganizationUrl.Resolve("~/context-missing", OrganizationCache.EmptySlug);

                response.Redirect(url, true);

                return;
            }

            if (!OrganizationCache.IsValidOrganization(slug))
            {
                var url = OrganizationUrl.Resolve("~/context-invalid", OrganizationCache.EmptySlug)
                          + $"?requested={HttpUtility.UrlEncode(slug)}";

                response.Redirect(url, true);

                return;
            }

            HttpContext.Current.Items[SlugVariable] = slug;
        }
    }
}