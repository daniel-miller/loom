using System.Configuration;
using System.Text.RegularExpressions;
using System.Web;

namespace Loom
{
    /// <summary>
    /// Determines the organization account (tenant) for the current HTTP request.
    /// </summary>
    public static class OrganizationResolver
    {
        /// <summary>IIS server variable populated by the URL rewrite rule in Web.config.</summary>
        public const string SlugServerVariable = "ORGANIZATION_SLUG";

        /// <summary>HttpContext.Items key for the resolved organization slug.</summary>
        public const string SlugItemKey = "Loom.OrganizationSlug";

        /// <summary>HttpContext.Items key for the resolved organization settings.</summary>
        public const string SettingsItemKey = "Loom.OrganizationSettings";

        private const string RemoteDomainSettingKey = "Loom.RemoteDomain";

        private const string LocalDomain = "localhost";

        private static readonly string RemoteDomain = LoadRemoteDomain();

        private static readonly Regex LegacySubdomainPattern = new Regex(
            @"^(?<environment>(?:local|sandbox|dev)-)?(?<organization>[a-z0-9-]+)\." + Regex.Escape(RemoteDomain) + "$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static string LoadRemoteDomain()
        {
            var value = ConfigurationManager.AppSettings[RemoteDomainSettingKey];

            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ConfigurationErrorsException(
                    $"Required appSettings key '{RemoteDomainSettingKey}' is missing or empty in Web.config.");
            }

            return value.Trim().ToLowerInvariant();
        }

        /// <summary>
        /// Validates required configuration. Call from <c>Application_Start</c> to fail fast
        /// on startup rather than on the first request.
        /// </summary>
        public static void EnsureConfigured()
        {
            // Touching the static field forces the type initializer to run,
            // which surfaces any ConfigurationErrorsException at startup.
            var _ = RemoteDomain;
        }

        public static void Resolve(HttpContextBase context)
        {
            if (context == null) throw new System.ArgumentNullException(nameof(context));

            var request = context.Request;
            var response = context.Response;

            // Already resolved in this request cycle (internal rewrite)

            if (context.Items.Contains(SlugItemKey))
                return;

            var slug = request.ServerVariables[SlugServerVariable];

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
                                ? LocalDomain
                                : $"{environment}.{RemoteDomain}";

                        var path = request.Url.PathAndQuery.TrimStart('/');

                        var redirectUrl = string.IsNullOrEmpty(path)
                            ? $"{request.Url.Scheme}://{targetHost}/{organization}"
                            : $"{request.Url.Scheme}://{targetHost}/{organization}/{path}";

                        RedirectAndComplete(context, redirectUrl);
                        return;
                    }
                }

                var url = OrganizationUrl.Resolve(context, "~/context-missing", OrganizationCache.EmptySlug);

                RedirectAndComplete(context, url);

                return;
            }

            if (!OrganizationCache.IsValidOrganization(slug))
            {
                var url = OrganizationUrl.Resolve(context, "~/context-invalid", OrganizationCache.EmptySlug)
                          + $"?requested={HttpUtility.UrlEncode(slug)}";

                RedirectAndComplete(context, url);

                return;
            }

            context.Items[SlugItemKey] = slug;
        }

        private static void RedirectAndComplete(HttpContextBase context, string url)
        {
            context.Response.Redirect(url, false);
            context.ApplicationInstance.CompleteRequest();
        }
    }
}