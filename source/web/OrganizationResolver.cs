using System.Text.RegularExpressions;
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

        private const string LocalDomain = "localhost";

        private const string RemoteDomain = "example.com";

        private static readonly Regex LegacySubdomainPattern = new Regex(
            @"^(?<environment>(?:local|sandbox|dev)-)?(?<organization>[a-z0-9-]+)\." + Regex.Escape(RemoteDomain) + "$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static void Resolve(HttpRequest request, HttpResponse response)
        {
            // Already resolved in this request cycle (internal rewrite)

            if (HttpContext.Current.Items.Contains(SlugVariable))
                return;

            var slug = request.ServerVariables[SlugVariable];

            if (string.IsNullOrEmpty(slug))
            {
                // Check for legacy subdomain pattern: environment-organization.example.com

                var host = request.Url.Host;

                var match = LegacySubdomainPattern.Match(host);

                if (match.Success)
                {
                    var organization = match.Groups["organization"].Value;

                    if (OrganizationCache.IsValidOrganization(organization))
                    {
                        // Redirect to path-based URL: environment.example.com/organization
                        // The local environment is a special case: localhost/organization

                        var environment = match.Groups["environment"].Value.TrimEnd('-');

                        var targetHost = string.IsNullOrEmpty(environment)
                            ? RemoteDomain
                            : environment == "local"
                                ? $"localhost"
                                : $"{environment}.{RemoteDomain}";

                        var path = request.Url.PathAndQuery.TrimStart('/');

                        var redirectUrl = string.IsNullOrEmpty(path)
                            ? $"{request.Url.Scheme}://{targetHost}/{organization}"
                            : $"{request.Url.Scheme}://{targetHost}/{organization}/{path}";

                        response.Redirect(redirectUrl, true);
                        return;
                    }
                }

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