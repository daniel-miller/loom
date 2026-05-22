using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Hosting;
using System.Xml;

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
            @"^(?<environment>(?:local|sandbox|dev)-)?(?<organization>"
                + OrganizationCache.SlugPatternSource
                + @")\." + Regex.Escape(RemoteDomain) + "$",
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
            _ = RemoteDomain;

            EnsureReservedSlugsInSyncWithWebConfig();
        }

        /// <summary>
        /// Verifies that <see cref="OrganizationCache.ReservedSlugs"/> matches the
        /// <c>ReservedSlugs</c> rewriteMap in Web.config. Reservation must agree at both
        /// layers; drift would silently let one layer treat a name as a tenant while the
        /// other treats it as app-scope.
        /// </summary>
        private static void EnsureReservedSlugsInSyncWithWebConfig()
        {
            var iisKeys = ReadReservedSlugsRewriteMap();

            var codeKeys = new HashSet<string>(
                OrganizationCache.ReservedSlugs,
                System.StringComparer.OrdinalIgnoreCase);

            var missingFromIis = codeKeys.Except(iisKeys, System.StringComparer.OrdinalIgnoreCase).ToArray();
            var missingFromCode = iisKeys.Except(codeKeys, System.StringComparer.OrdinalIgnoreCase).ToArray();

            if (missingFromIis.Length == 0 && missingFromCode.Length == 0)
                return;

            var detail = new System.Text.StringBuilder();
            detail.Append("OrganizationCache.ReservedSlugs is out of sync with the ReservedSlugs rewriteMap in Web.config.");

            if (missingFromIis.Length > 0)
                detail.Append(" Missing from Web.config: ").Append(string.Join(", ", missingFromIis)).Append('.');

            if (missingFromCode.Length > 0)
                detail.Append(" Missing from OrganizationCache.ReservedSlugs: ").Append(string.Join(", ", missingFromCode)).Append('.');

            throw new ConfigurationErrorsException(detail.ToString());
        }

        private static HashSet<string> ReadReservedSlugsRewriteMap()
        {
            var keys = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase);

            var webConfigPath = Path.Combine(
                HostingEnvironment.ApplicationPhysicalPath ?? System.AppDomain.CurrentDomain.BaseDirectory,
                "Web.config");

            if (!File.Exists(webConfigPath))
                throw new ConfigurationErrorsException($"Web.config not found at '{webConfigPath}'.");

            var doc = new XmlDocument();
            doc.Load(webConfigPath);

            var nodes = doc.SelectNodes(
                "/configuration/system.webServer/rewrite/rewriteMaps/rewriteMap[@name='ReservedSlugs']/add");

            if (nodes == null || nodes.Count == 0)
                throw new ConfigurationErrorsException(
                    "Web.config is missing the <rewriteMap name=\"ReservedSlugs\"> entry expected by OrganizationResolver.");

            foreach (XmlNode node in nodes)
            {
                var key = node.Attributes?["key"]?.Value;
                if (!string.IsNullOrEmpty(key))
                    keys.Add(key);
            }

            return keys;
        }

        public static void Resolve(HttpContextBase context)
        {
            if (context == null) throw new System.ArgumentNullException(nameof(context));

            // Already resolved earlier in this request cycle (internal rewrite).
            if (context.Items.Contains(SlugItemKey))
                return;

            var slug = context.Request.ServerVariables[SlugServerVariable];

            if (string.IsNullOrEmpty(slug))
            {
                HandleNoSlug(context);
                return;
            }

            if (!OrganizationCache.IsValidOrganization(slug))
            {
                RedirectToContextInvalid(context, slug);
                return;
            }

            StoreTenantContext(context, slug);
        }

        private static void HandleNoSlug(HttpContextBase context)
        {
            // GitHub-style convention: when IIS did not set a tenant slug, the
            // first URL segment may be a reserved app-scope path. Reserved paths
            // resolve in app-scope (no tenant context).
            if (IsReservedFirstSegment(context.Request.Url))
                return;

            if (TryHandleLegacySubdomainRedirect(context))
                return;

            var url = OrganizationUrl.Resolve(context, "~/context-missing", OrganizationCache.EmptySlug);
            RedirectAndComplete(context, url);
        }

        private static bool TryHandleLegacySubdomainRedirect(HttpContextBase context)
        {
            var request = context.Request;
            var match = LegacySubdomainPattern.Match(request.Url.Host);
            if (!match.Success) return false;

            var organization = match.Groups["organization"].Value;
            if (!OrganizationCache.IsValidOrganization(organization)) return false;

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
            return true;
        }

        private static void RedirectToContextInvalid(HttpContextBase context, string requestedSlug)
        {
            var url = OrganizationUrl.Resolve(context, "~/context-invalid", OrganizationCache.EmptySlug)
                      + $"?requested={HttpUtility.UrlEncode(requestedSlug)}";

            RedirectAndComplete(context, url);
        }

        private static void StoreTenantContext(HttpContextBase context, string slug)
        {
            // Snapshot settings now so a concurrent cache reload cannot cause a
            // mid-request KeyNotFoundException when the page reads them.
            var settings = OrganizationCache.GetBySlug(slug);

            context.Items[SlugItemKey] = slug;
            context.Items[SettingsItemKey] = settings;
        }

        private static void RedirectAndComplete(HttpContextBase context, string url)
        {
            context.Response.Redirect(url, false);
            context.ApplicationInstance.CompleteRequest();
        }

        private static bool IsReservedFirstSegment(System.Uri url)
        {
            var path = url?.AbsolutePath;
            if (string.IsNullOrEmpty(path) || path == "/")
                return false;

            // Extract first segment between leading '/' and the next '/' (or end).
            var start = path[0] == '/' ? 1 : 0;
            var end = path.IndexOf('/', start);
            var first = end < 0 ? path.Substring(start) : path.Substring(start, end - start);

            return OrganizationCache.IsReservedSlug(first);
        }
    }
}